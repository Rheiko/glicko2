Public Class Form1
    '==========================================================================================================
    '====================== "KALKULATOR GLICKO-2" BY CHRISTIAN RHEINARD SADEKO (2021) =========================
    '==========================================================================================================
    '=  == INTRO ==
    '=  PROGRAM INI DIBUAT UNTUK MEMPERMUDAH PENGGUNA DALAM MENGHITUNG NILAI RATING MENGGUNAKAN KONSEP GLICKO-2
    '=  SISTEM GLICKO-2 MERUPAKAN PENEMUAN PROFESOR MARK E. GLICKMAN DARI BOSTON UNIVERSITY, 20 NOVEMBER 2013
    '=
    '=  TERDAPAT 3 VARIABEL PENTING YANG DIGUNAKAN YAITU: RATING, RATING DEVIATION, DAN VOLATILITY
    '=
    '=  RUMUS DARI PROGRAM INI MENGIKUTI INSTRUKSI YANG DIJELASKAN DALAM JURNAL GLICKO-2 PADA:
    '=  http://www.glicko.net/glicko/glicko2.pdf
    '=  
    '=  == DISCLAIMER ==
    '=  KETEPATAN RUMUS TIDAK DIJAMIN PASTI, GUNAKAN DENGAN RESIKO YANG ANDA TANGGUNG SENDIRI
    '=
    '=  == CARA KERJA PROGRAM (END-USER) ==
    '=  - TENTUKAN JUMLAH LAWAN DAN TEKAN TOMBOL KONFIRMASI
    '=  - ISI SEMUA FIELD SESUAI KETERANGAN YANG ADA
    '=  - TEKAN TOMBOL "CALCULATE" UNTUK MENGHITUNG PEROLEHAN NILAI RATING
    '=
    '=  == CARA KERJA PROGRAM (DEVELOPER) ==
    '=  TOMBOL KONFIRMASI AKAN MEMICU FUNGSI PENAMBAHAN INPUT, ENABLE TOMBOL RESET, DISABLE TOMBOL KONFIRMASI
    '=  FUNGSI PENAMBAHAN INPUT AKAN MEMBUAT KONTROL DINAMIS PADA FORM
    '=  TOMBOL RESET AKAN MENGHAPUS SEMUA KONTROL DINAMIS YANG ADA, ENABLE TOMBOL KONFIRMASI, DISABLE TOMBOL RESET
    '=  TOMBOL CALCULATE AKAN MEMICU PROSEDUR PERHITUNGAN GLICKO-2
    '=  
    '=  == DOWNLOAD ==
    '=  https://github.com/Rheiko/glicko2
    '=
    '=  == END OF DOCUMENTATION ==
    '=
    '==========================================================================================================

    '==========================================================================================================
    '==========================================================================================================
    '========== PERHITUNGAN GLICKO-2 !!!
    '==========================================================================================================
    '==========================================================================================================

    Const t = 0.5
    '==========================================================================================================
    '========== DEKLARASI VARIABEL
    '==========================================================================================================
    Dim Rating(), RatDev(), Volat(), winlose() As Double 'NILAI YANG DIINPUT
    Dim r(), rd(), v, delta, epsilon As Double 'NILAI YANG DIPROSES
    Dim g(), exp(), A, B, C, fA, fB, fC, k, small_a As Double 'NILAI YANG DIPROSES

    Sub GlickoCalc(ByVal count As Integer)
        ReDim r(count), rd(count), g(count), exp(count) 'MENGUBAH UKURAN ARRAY DINAMIS
        '=============================
        '===== INISIALISASI AWAL =====
        '=============================
        v = 0
        delta = 0
        '===========================================================
        '===== STEP 2 ===== KONVERSI RATING DAN RD KE SKALA GLICKO-2
        '===========================================================
        For i As Integer = 0 To count
            r(i) = (Rating(i) - 1500) / 173.7178
            rd(i) = RatDev(i) / 173.7178

            If (i >= 1) Then
                g(i) = 1 / Math.Sqrt(1 + 3 * Math.Pow(rd(i), 2) / Math.Pow(Math.PI, 2))
                exp(i) = 1 / (1 + Math.Exp(-g(i) * (r(0) - r(i)))) 'r(0) = player | r(1..i) = opponents

                '===============================================================================================
                '===== STEP 3 ===== MENGHITUNG NILAI ESTIMASI VARIANTS DARI RATING PLAYER BERDASARKAN HASIL GAME
                '===============================================================================================
                v = v + Math.Pow(g(i), 2) * exp(i) * (1 - exp(i))
                '=========================================================================================
                '===== STEP 4 ===== MENGHITUNG NILAI DELTA ATAU ESTIMASI PERKEMBANGAN NILAI RATING DENGAN
                '================== MEMBANDINGKAN NILAI RATING SEBELUM PERHITUNGAN (PRE-RATING PERIOD) 
                '================== DENGAN RATING BERDASARKAN HASIL GAME
                '=========================================================================================
                delta = delta + g(i) * (winlose(i) - exp(i))
            End If

            'MessageBox.Show("r(" & i & ") = " & r(i).ToString & ", rd(" & i & ") = " & rd(i).ToString) '//DEBUG
            'MessageBox.Show("g(" & i & ") = " & g(i).ToString & ", E(" & i & ") = " & exp(i).ToString) '//DEBUG
        Next
        v = Math.Pow(v, -1)
        delta = delta * v

        'MessageBox.Show("v = " & v.ToString & ", delta = " & delta.ToString) '//DEBUG

        '========================================================
        '===== STEP 5 ===== MENENTUKAN NILAI BARU DARI VOLATILITY
        '========================================================
        epsilon = 0.000001
        small_a = Math.Log(Math.Pow(Volat(0), 2)) 'Math.Log10(Math.Pow(Volat(0), 2)) / Math.Log10(Math.E) 'ekuivalen ln(volat^2)
        A = small_a

        If Math.Pow(delta, 2) > Math.Pow(rd(0), 2) + v Then
            B = Math.Log10((Math.Pow(delta, 2) - Math.Pow(rd(0), 2) - v)) / Math.Log10(Math.E)
        ElseIf Math.Pow(delta, 2) <= Math.Pow(rd(0), 2) + v Then
            k = 1
            If compute(small_a - k * t) < 0 Then
                k = k + 1
            End If
            B = small_a - k * t
        End If
        MessageBox.Show("A = " & A.ToString & ", B = " & B.ToString)
        fA = compute(A)
        fB = compute(B)
        While (Math.Abs(B - A)) > epsilon
            C = A + (A - B) * fA / (fB - fA)
            fC = compute(C)
            If fC * fB < 0 Then
                A = B
                fA = fB
            Else
                fA = fA / 2
            End If
            B = C
            fB = fC
        End While
        MessageBox.Show("A = " & A.ToString & ", B = " & B.ToString)

        Volat(0) = Math.Pow(Math.E, A / 2)

        '======================================================================================
        '===== STEP 6 ===== UPDATE NILAI RD KE NILAI RD SEBELUM PERHITUNGAN (PRE-RATING PERIOD)
        '======================================================================================
        rd(0) = Math.Sqrt(Math.Pow(rd(0), 2) + Math.Pow(Volat(0), 2)) 'RD pre-rating period

        '===============================================================================
        '===== STEP 7 ===== UPDATE NILAI RATING DAN RD KE NILAI BARU YANG TELAH DIHITUNG
        '===============================================================================
        rd(0) = 1 / Math.Sqrt((1 / Math.Pow(rd(0), 2)) + (1 / v)) 'RD Baru

        Dim TempR As Double = 0
        For i As Integer = 1 To count
            TempR = TempR + (g(i) * (winlose(i) - exp(i)))
        Next

        r(0) = r(0) + Math.Pow(rd(0), 2) * TempR 'R Baru

        '===================================================================
        '===== STEP 8 ===== KONVERSI RATING DAN RD KEMBALI KE SKALA ORIGINAL
        '===================================================================
        r(0) = r(0) * 173.7178 + 1500
        rd(0) = rd(0) * 173.7178

        'MessageBox.Show("R = " & r(0).ToString & ", RD = " & rd(0).ToString) '//DEBUG
    End Sub

    Function compute(ByVal x As Double) As Double
        Dim f, t As Double

        f = ((Math.Pow(Math.E, x) * (Math.Pow(delta, 2) - Math.Pow(rd(0), 2) - v - Math.Pow(Math.E, x))) / (2 * Math.Pow((Math.Pow(rd(0), 2) + v + Math.Pow(Math.E, x)), 2))) - ((x - A) / Math.Pow(t, 2))
        Return f
    End Function

    '==========================================================================================================
    '========== FUNGSI PENAMBAHAN INPUT LAWAN
    '==========================================================================================================
    Dim cLeft As Integer = 1
    Dim MyControls As List(Of Control)
    Public Function AddNewTextBox()
        Dim lbl_lawan As New System.Windows.Forms.Label()
        Dim lbl_rating As New System.Windows.Forms.Label()
        Dim lbl_rd As New System.Windows.Forms.Label()
        Dim lbl_vol As New System.Windows.Forms.Label()
        Dim lbl_winlose As New System.Windows.Forms.Label()
        Dim txt_rating As New System.Windows.Forms.NumericUpDown
        Dim txt_rd As New System.Windows.Forms.NumericUpDown
        Dim txt_vol As New System.Windows.Forms.NumericUpDown
        Dim cb_winlose As New System.Windows.Forms.CheckBox
        With MyControls
            .Add(lbl_lawan)
            .Add(lbl_rating)
            .Add(lbl_rd)
            .Add(lbl_vol)
            .Add(lbl_winlose)
            .Add(txt_rating)
            .Add(txt_rd)
            .Add(txt_vol)
            .Add(cb_winlose)
        End With
        With Panel1
            .Controls.Add(lbl_lawan)
            .Controls.Add(lbl_rating)
            .Controls.Add(lbl_rd)
            .Controls.Add(lbl_vol)
            .Controls.Add(lbl_winlose)
            .Controls.Add(txt_rating)
            .Controls.Add(txt_rd)
            .Controls.Add(txt_vol)
            .Controls.Add(cb_winlose)
        End With
        lbl_lawan.Top = cLeft * 25 + ((cLeft - 1) * 125)
        lbl_lawan.Left = 20
        lbl_lawan.Text = "Lawan " & Me.cLeft.ToString
        lbl_rating.Top = cLeft * 25 + 25 + ((cLeft - 1) * 125)
        lbl_rating.Left = 20
        lbl_rating.Text = "Rating"
        lbl_rd.Top = cLeft * 25 + 50 + ((cLeft - 1) * 125)
        lbl_rd.Left = 20
        lbl_rd.Text = "Rating Deviation"
        lbl_vol.Top = cLeft * 25 + 75 + ((cLeft - 1) * 125)
        lbl_vol.Left = 20
        lbl_vol.Text = "Volatility"
        lbl_winlose.Top = cLeft * 25 + 100 + ((cLeft - 1) * 125)
        lbl_winlose.Left = 20
        lbl_winlose.Text = "Win?"
        txt_rating.Top = cLeft * 25 + 25 + ((cLeft - 1) * 125)
        txt_rating.Left = 150
        txt_rating.Maximum = 10000
        txt_rating.Name = "txt_rating" & Me.cLeft.ToString
        txt_rd.Top = cLeft * 25 + 50 + ((cLeft - 1) * 125)
        txt_rd.Left = 150
        txt_rd.Maximum = 10000
        txt_rd.Name = "txt_rd" & Me.cLeft.ToString
        txt_vol.Top = cLeft * 25 + 75 + ((cLeft - 1) * 125)
        txt_vol.Left = 150
        txt_vol.DecimalPlaces = 2
        txt_vol.Name = "txt_vol" & Me.cLeft.ToString
        cb_winlose.Top = cLeft * 25 + 100 + ((cLeft - 1) * 125)
        cb_winlose.Left = 150
        cb_winlose.Name = "cb_winlose" & Me.cLeft.ToString
        cLeft += 1
        Return Panel1.Controls
    End Function


    '==========================================================================================================
    '========== TOMBOL KONFIRMASI
    '==========================================================================================================
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Dim JumlahLawan As Integer = num_count.Value
        MyControls = New List(Of Control)
        For i As Integer = 0 To JumlahLawan - 1
            AddNewTextBox()
        Next
        Button1.Enabled = False
        Button2.Enabled = True
        Button3.Enabled = True
    End Sub

    '==========================================================================================================
    '========== TOMBOL RESET
    '==========================================================================================================
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        For Each c As Control In MyControls
            Me.Controls.Remove(c)
            c.Dispose()
        Next
        cLeft = 1
        Button1.Enabled = True
        Button2.Enabled = False
        Button3.Enabled = False
    End Sub

    '==========================================================================================================
    '========== TOMBOL HITUNG
    '==========================================================================================================
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        Dim JumlahLawan As Integer = num_count.Value
        ReDim Rating(JumlahLawan), RatDev(JumlahLawan), Volat(JumlahLawan), winlose(JumlahLawan)
        Dim win As Integer = 0
        Rating(0) = p_rating.Value
        RatDev(0) = p_rd.Value
        Volat(0) = p_vol.Value
        For i As Integer = 1 To JumlahLawan
            Dim txt_rating As NumericUpDown = Panel1.Controls.Item("txt_rating" & i)
            Dim txt_rd As NumericUpDown = Panel1.Controls.Item("txt_rd" & i)
            Dim txt_vol As NumericUpDown = Panel1.Controls.Item("txt_vol" & i)
            Dim cb_winlose As CheckBox = Panel1.Controls.Item("cb_winlose" & i)
            Rating(i) = txt_rating.Value
            RatDev(i) = txt_rd.Value
            Volat(i) = txt_vol.Value
            If cb_winlose.Checked = True Then
                win = 1
            Else
                win = 0
            End If
            winlose(i) = win
        Next
        txt_keterangan.AppendText("===== PERHITUNGAN =====" & Environment.NewLine &
                                  "Rating Awal: " & Rating(0) & Environment.NewLine &
                                  "RD Awal: " & RatDev(0) & Environment.NewLine &
                                  "Volatility Awal: " & Volat(0) & Environment.NewLine
                                  )
        GlickoCalc(JumlahLawan)
        txt_keterangan.AppendText("========================" & Environment.NewLine &
                                  "Rating Akhir: " & r(0) & Environment.NewLine &
                                  "RD Akhir: " & rd(0) & Environment.NewLine &
                                  "Volatility Akhir: " & Volat(0) & Environment.NewLine & Environment.NewLine
                                  )
    End Sub



End Class
