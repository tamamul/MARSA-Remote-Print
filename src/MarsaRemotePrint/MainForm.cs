
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MarsaRemotePrint;

public sealed class MainForm : Form
{
    private const string DefaultBaseUrl =
        "https://chat.marsa9.com/printserver-ci4/public/";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(90)
    };

    private readonly TextBox _baseUrl =
        new() { Text = DefaultBaseUrl, Dock = DockStyle.Fill };

    private readonly ComboBox _printer =
        new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };

    private readonly TextBox _pdfPath =
        new() { ReadOnly = true, Dock = DockStyle.Fill };

    private readonly TextBox _pin =
        new() { MaxLength = 5, UseSystemPasswordChar = true, Dock = DockStyle.Fill };

    private readonly NumericUpDown _copies =
        new() { Minimum = 1, Maximum = 10, Value = 1, Width = 100 };

    private readonly Button _choosePdf =
        new() { Text = "Pilih PDF...", AutoSize = true };

    private readonly Button _refreshPrinters =
        new() { Text = "Muat ulang printer", AutoSize = true };

    private readonly Button _submit =
        new() { Text = "Kirim untuk dicetak", AutoSize = true };

    private readonly Label _status =
        new() { AutoSize = true, Text = "Menghubungkan ke server..." };

    private readonly LinkLabel _statusLink =
        new() { AutoSize = true, Visible = false };

    private string? _selectedPdf;

    public MainForm()
    {
        Text = "MARSA Remote Print";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(700, 500);
        Size = new Size(790, 560);
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(246, 248, 250);

        var title = new Label
        {
            Text = "MARSA Remote Print",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.FromArgb(25, 85, 50)
        };

        var subtitle = new Label
        {
            Text = "Kirim dokumen PDF ke printer sekolah",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 0, 0, 16)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 2,
            RowCount = 8
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        for (int i = 1; i < 8; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        layout.Controls.Add(header, 0, 0);
        layout.SetColumnSpan(header, 2);

        AddRow(layout, 1, "Alamat server", _baseUrl);

        var printerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true
        };
        printerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        printerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        printerPanel.Controls.Add(_printer, 0, 0);
        printerPanel.Controls.Add(_refreshPrinters, 1, 0);
        AddRow(layout, 2, "Printer", printerPanel);

        var pdfPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true
        };
        pdfPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pdfPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pdfPanel.Controls.Add(_pdfPath, 0, 0);
        pdfPanel.Controls.Add(_choosePdf, 1, 0);
        AddRow(layout, 3, "Dokumen PDF", pdfPanel);

        AddRow(layout, 4, "PIN cetak (5 digit)", _pin);
        AddRow(layout, 5, "Jumlah salinan", _copies);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 4)
        };

        _submit.Height = 38;
        _submit.Padding = new Padding(12, 4, 12, 4);
        _submit.BackColor = Color.FromArgb(27, 94, 32);
        _submit.ForeColor = Color.White;
        _submit.FlatStyle = FlatStyle.Flat;
        actionPanel.Controls.Add(_submit);
        AddRow(layout, 6, "", actionPanel);

        var resultPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        resultPanel.Controls.Add(_status);
        resultPanel.Controls.Add(_statusLink);
        AddRow(layout, 7, "Status", resultPanel);

        Controls.Add(layout);

        _choosePdf.Click += (_, _) => ChoosePdf();
        _refreshPrinters.Click += async (_, _) => await LoadPrintersAsync();
        _submit.Click += async (_, _) => await SubmitAsync();

        _statusLink.LinkClicked += (_, _) =>
        {
            if (_statusLink.Tag is string url &&
                Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(uri.ToString())
                    {
                        UseShellExecute = true
                    });
            }
        };

        Shown += async (_, _) => await LoadPrintersAsync();
    }

    private static void AddRow(
        TableLayoutPanel table,
        int row,
        string labelText,
        Control control)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 8)
        };

        control.Margin = new Padding(0, 4, 0, 4);

        if (control is TextBox textBox)
            textBox.MinimumSize = new Size(0, 30);

        if (control is ComboBox combo)
            combo.MinimumSize = new Size(0, 30);

        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private string GetBaseUrl()
    {
        var value = _baseUrl.Text.Trim();

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                "Alamat server belum diisi.");

        if (!value.EndsWith('/'))
            value += "/";

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
             uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                "Alamat server harus berupa URL HTTP/HTTPS yang valid.");
        }

        return value;
    }

    private void ChoosePdf()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Pilih dokumen PDF",
            Filter = "Dokumen PDF (*.pdf)|*.pdf",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _selectedPdf = dialog.FileName;
            _pdfPath.Text = _selectedPdf;
        }
    }

    private async Task LoadPrintersAsync()
    {
        try
        {
            SetBusy(true, "Memuat daftar printer...");

            using var response = await Http.GetAsync(
                GetBaseUrl() + "api/teacher/printers");

            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    ReadMessage(root,
                        $"Server mengembalikan HTTP {(int)response.StatusCode}."));
            }

            if (!root.TryGetProperty("printers", out var list) ||
                list.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "Respons server tidak memiliki daftar printer.");
            }

            var items = new List<PrinterItem>();

            foreach (var item in list.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idValue)
                    ? idValue.ToString()
                    : "";

                var name = item.TryGetProperty("nama_printer", out var nameValue)
                    ? nameValue.GetString() ?? "Printer"
                    : "Printer";

                var location = item.TryGetProperty("lokasi", out var locationValue)
                    ? locationValue.GetString() ?? ""
                    : "";

                if (int.TryParse(id, out var numericId))
                    items.Add(new PrinterItem(numericId, name, location));
            }

            _printer.DataSource = null;
            _printer.DataSource = items;
            _printer.DisplayMember = nameof(PrinterItem.Display);
            _printer.ValueMember = nameof(PrinterItem.Id);

            SetStatus(items.Count == 0
                ? "Tidak ada printer online. Periksa agent printer."
                : $"{items.Count} printer online tersedia.");
        }
        catch (Exception ex)
        {
            SetStatus("Gagal memuat printer: " + ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task SubmitAsync()
    {
        try
        {
            if (_printer.SelectedItem is not PrinterItem selectedPrinter)
                throw new InvalidOperationException("Pilih printer terlebih dahulu.");

            if (string.IsNullOrWhiteSpace(_selectedPdf) ||
                !File.Exists(_selectedPdf))
            {
                throw new InvalidOperationException(
                    "Pilih file PDF terlebih dahulu.");
            }

            var pin = _pin.Text.Trim();

            if (pin.Length != 5 || pin.Any(c => c < '0' || c > '9'))
                throw new InvalidOperationException(
                    "PIN harus terdiri dari tepat 5 angka.");

            var fileInfo = new FileInfo(_selectedPdf);

            if (fileInfo.Length <= 0 ||
                fileInfo.Length > 20 * 1024 * 1024)
            {
                throw new InvalidOperationException(
                    "Ukuran PDF harus lebih dari 0 dan maksimal 20 MiB.");
            }

            SetBusy(true, "Mengirim dokumen ke antrean cetak...");

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(pin), "pin");
            form.Add(new StringContent(
                selectedPrinter.Id.ToString(CultureInfo.InvariantCulture)),
                "printer_id");
            form.Add(new StringContent(
                ((int)_copies.Value).ToString(CultureInfo.InvariantCulture)),
                "jumlah_copy");

            await using var stream = File.OpenRead(_selectedPdf);
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");

            form.Add(fileContent, "pdf", Path.GetFileName(_selectedPdf));

            using var response = await Http.PostAsync(
                GetBaseUrl() + "api/teacher/jobs", form);

            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!response.IsSuccessStatusCode ||
                !root.TryGetProperty("ok", out var ok) ||
                ok.ValueKind != JsonValueKind.True)
            {
                throw new InvalidOperationException(
                    ReadMessage(root,
                        $"Pengiriman gagal (HTTP {(int)response.StatusCode})."));
            }

            var message = root.TryGetProperty("message", out var msg)
                ? msg.GetString() ?? "Dokumen masuk antrean."
                : "Dokumen masuk antrean.";

            var jobId = "-";
            var printerName = selectedPrinter.Name;

            if (root.TryGetProperty("job", out var job))
            {
                if (job.TryGetProperty("id", out var idValue))
                    jobId = idValue.ToString();

                if (job.TryGetProperty("printer_name", out var printerValue))
                    printerName = printerValue.GetString() ?? printerName;

                if (job.TryGetProperty("status_url", out var statusValue))
                {
                    var statusUrl = statusValue.GetString();

                    // Jangan membuka URL status yang bukan berasal
                    // dari host server yang dikonfigurasi.
                    if (Uri.TryCreate(statusUrl, UriKind.Absolute, out var statusUri) &&
                        Uri.TryCreate(GetBaseUrl(), UriKind.Absolute, out var baseUri) &&
                        statusUri.Scheme == Uri.UriSchemeHttps &&
                        statusUri.Host.Equals(baseUri.Host,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        _statusLink.Text = "Buka status pekerjaan";
                        _statusLink.Tag = statusUri.ToString();
                        _statusLink.Visible = true;
                    }
                }
            }

            SetStatus(
                $"{message} Job #{jobId} · {printerName} · {(int)_copies.Value} salinan.");

            _pin.Clear();

            MessageBox.Show(
                this,
                $"Dokumen berhasil dikirim.\nNomor pekerjaan: #{jobId}\nPrinter: {printerName}",
                "MARSA Remote Print",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (JsonException)
        {
            SetStatus(
                "Respons server tidak terbaca. Periksa URL API atau log server.");
        }
        catch (Exception ex)
        {
            SetStatus("Gagal mengirim: " + ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static string ReadMessage(JsonElement root, string fallback)
    {
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("message", out var message) &&
            message.ValueKind == JsonValueKind.String)
        {
            return message.GetString() ?? fallback;
        }

        return fallback;
    }

    private void SetStatus(string message)
    {
        _status.Text = message;
    }

    private void SetBusy(bool busy, string? message = null)
    {
        _refreshPrinters.Enabled = !busy;
        _choosePdf.Enabled = !busy;
        _submit.Enabled = !busy;
        _printer.Enabled = !busy;
        _baseUrl.Enabled = !busy;
        _pin.Enabled = !busy;
        _copies.Enabled = !busy;

        if (message is not null)
            SetStatus(message);

        UseWaitCursor = busy;
    }

    private sealed record PrinterItem(int Id, string Name, string Location)
    {
        public string Display =>
            string.IsNullOrWhiteSpace(Location)
                ? Name
                : $"{Name} — {Location}";

        public override string ToString() => Display;
    }
}
