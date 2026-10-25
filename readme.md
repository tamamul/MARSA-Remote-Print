
# MARSA Remote Print

Aplikasi desktop native Windows untuk mengirim dokumen PDF ke printer sekolah melalui server MARSA.

## Fitur

- Memuat daftar printer online dari server
- Memilih dokumen PDF
- PIN cetak 5 digit
- Jumlah salinan 1–10
- Mengirim dokumen ke antrean cetak
- Menampilkan nomor pekerjaan
- Build installer Windows melalui GitHub Actions

## Backend

Alamat server default:

https://chat.marsa9.com/printserver-ci4/public/

Endpoint:
- GET `api/teacher/printers`
- POST `api/teacher/jobs`

## Teknologi

- C# WinForms
- .NET 10
- Inno Setup
- GitHub Actions

## Build lokal

Instal .NET 10 SDK, kemudian jalankan:

```powershell
dotnet restore src/MarsaRemotePrint/MarsaRemotePrint.csproj
dotnet build src/MarsaRemotePrint/MarsaRemotePrint.csproj -c Release
```

## Build installer

1. Upload semua file proyek ke repositori GitHub `MARSA-Remote-Print`.
2. Buka tab Actions.
3. Pilih workflow Build MARSA Remote Print Installer.
4. Tekan Run workflow.
5. Setelah build berhasil, buka hasil workflow.
6. Unduh artifact `MARSA-Remote-Print-Installer`.

File installer yang dihasilkan:

`MARSA-Remote-Print-Setup.exe`
