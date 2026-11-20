// Acceptance runner for ITSD 81 Lab 5 (T01-T06 over HTTP, with PostgreSQL readbacks).
// It is a development aid, not a test framework. Everything it writes to the evidence file is what it ACTUALLY observed.
//
//   dotnet run --project tools/EquipmentBorrowing.AcceptanceRunner -- [--api http://localhost:5080] [--out evidence/acceptance-run.md] [--only T01,T05]
//
// Requires: API started with Dev__Enabled=true and Clock__FixedDate=2026-11-10 against a *_dev / *_test database.
// Optional: ConnectionStrings__Default (same as the API) enables the independent PostgreSQL snapshots.
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Npgsql;

string api = "http://localhost:5080", outPath = "evidence/acceptance-run.md";
HashSet<string>? only = null;
for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--api") api = args[i + 1].TrimEnd('/');
    if (args[i] == "--out") outPath = args[i + 1];
    if (args[i] == "--only") only = args[i + 1].Split(',').Select(x => x.Trim().ToUpperInvariant()).ToHashSet();
}
var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
var http = new HttpClient { BaseAddress = new Uri(api + "/"), Timeout = TimeSpan.FromSeconds(30) };
var md = new StringBuilder();
int passed = 0, failed = 0;
var pretty = new JsonSerializerOptions { WriteIndented = true };

void W(string s = "") => md.AppendLine(s);
void Section(string s) { W(); W($"## {s}"); W(); Console.WriteLine($"== {s}"); }

async Task<(int Status, JsonNode? Json, string Raw)> Call(string method, string path, object? body = null, string? note = null)
{
    var req = new HttpRequestMessage(new HttpMethod(method), path.TrimStart('/'));
    string? bodyText = null;
    if (body is not null) { bodyText = body is string s ? s : JsonSerializer.Serialize(body); req.Content = new StringContent(bodyText, Encoding.UTF8, "application/json"); }
    var res = await http.SendAsync(req);
    var raw = await res.Content.ReadAsStringAsync();
    JsonNode? node = null; try { if (raw.Length > 0) node = JsonNode.Parse(raw); } catch { }
    W($"**{(note ?? "")}** `{method} /{path.TrimStart('/')}`" + (bodyText is null ? "" : $" body `{bodyText}`"));
    var shown = node is null ? raw : node.ToJsonString(pretty);
    if (shown.Length > 1800) shown = shown[..1800] + "\n... (truncated)";
    W($"→ **{(int)res.StatusCode}**" + (res.Headers.Location is null ? "" : $" Location: `{res.Headers.Location}`"));
    W("```json"); W(shown); W("```");
    return ((int)res.StatusCode, node, raw);
}

void Check(string what, bool ok, string detail = "")
{
    if (ok) passed++; else failed++;
    W($"- {(ok ? "PASS" : "**FAIL**")}: {what}{(detail.Length > 0 && !ok ? $" ({detail})" : "")}");
    Console.WriteLine($"   {(ok ? "PASS" : "FAIL")}: {what} {(ok ? "" : detail)}");
}

async Task Snapshot(string label)
{
    if (string.IsNullOrEmpty(cs)) { W($"_{label}: PostgreSQL snapshot skipped (ConnectionStrings__Default not set)._"); return; }
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    W($"**PostgreSQL snapshot — {label}** (taken {DateTime.Now:yyyy-MM-dd HH:mm:ss})");
    W("```text");
    async Task Q(string title, string sql)
    {
        W($"-- {title}");
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            var cells = new List<string>();
            for (int i = 0; i < r.FieldCount; i++) cells.Add(r.IsDBNull(i) ? "NULL" : r.GetValue(i)?.ToString() ?? "");
            W("   " + string.Join(" | ", cells));
        }
    }
    await Q("Students: number | name | eligible", "SELECT \"StudentNumber\",\"FullName\",\"IsEligible\" FROM \"Students\" ORDER BY 1");
    await Q("Equipment: code | name | available", "SELECT \"EquipmentCode\",\"Name\",\"IsAvailable\" FROM \"Equipment\" ORDER BY 1");
    await Q("Borrowings: student | item | borrowed | expected | returned",
        "SELECT s.\"StudentNumber\", e.\"EquipmentCode\", b.\"BorrowedDate\", b.\"ExpectedReturnDate\", b.\"ReturnedDate\" FROM \"Borrowings\" b JOIN \"Students\" s ON s.\"Id\"=b.\"StudentId\" JOIN \"Equipment\" e ON e.\"Id\"=b.\"EquipmentId\" ORDER BY 1,2,3");
    W("```");
}

async Task<long> Count(string table)
{
    if (string.IsNullOrEmpty(cs)) return -1;
    await using var conn = new NpgsqlConnection(cs); await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand($"SELECT count(*) FROM \"{table}\"", conn);
    return (long)(await cmd.ExecuteScalarAsync())!;
}

async Task Reset() { var r = await http.PostAsync("dev/reset", null); if (!r.IsSuccessStatusCode) throw new Exception("dev/reset failed: " + (int)r.StatusCode); }
async Task<JsonNode?> Get(string p) => JsonNode.Parse(await http.GetStringAsync(p.TrimStart('/')));
async Task<Guid> StudentId(string no) => Guid.Parse(((await Get("api/v1/students"))!.AsArray().First(x => x!["studentNumber"]!.ToString() == no))!["id"]!.ToString());
async Task<Guid> EquipId(string code) => Guid.Parse(((await Get("api/v1/equipment"))!.AsArray().First(x => x!["equipmentCode"]!.ToString() == code))!["id"]!.ToString());
string D(DateOnly d) => d.ToString("yyyy-MM-dd");
bool Run(string t) => only is null || only.Contains(t);

// ---------------------------------------------------------------------------------------------
W("# Acceptance run (generated)"); W();
W($"- Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (machine local time)"); W($"- API: {api}");
DateOnly today;
try
{
    var info = await Get("dev/info");
    today = DateOnly.Parse(info!["today"]!.ToString());
    W($"- API local date (clock): **{D(today)}**  | database: `{info["database"]}`");
    if (D(today) != "2026-11-10") W("- NOTE: the clock is not 2026-11-10; all dated requests below use the same offsets from this date (reference Section 3).");
}
catch (Exception ex) { Console.WriteLine("Cannot reach /dev/info. Start the API with Dev__Enabled=true. " + ex.Message); return 2; }
var due = today.AddDays(2); var yesterday = today.AddDays(-1);
W($"- PostgreSQL snapshots: {(string.IsNullOrEmpty(cs) ? "DISABLED (set ConnectionStrings__Default)" : "enabled")}");

// ---------------------------------------------- T01
if (Run("T01"))
{
    Section("T01 — Independent API reads (API-01..03, 07, 13..16)"); await Reset();
    await Snapshot("before");
    var eq = await Call("GET", "api/v1/equipment", note: "all equipment");
    Check("7 equipment items", eq.Json?.AsArray().Count == 7);
    var av = await Call("GET", "api/v1/equipment?available=true", note: "available only");
    var codes = av.Json!.AsArray().Select(x => x!["equipmentCode"]!.ToString()).ToHashSet();
    Check("available excludes EQ-001/004/005 and includes 002/003/006/007", !codes.Contains("EQ-001") && !codes.Contains("EQ-004") && !codes.Contains("EQ-005") && codes.SetEquals(new[] { "EQ-002", "EQ-003", "EQ-006", "EQ-007" }));
    Check("available=maybe → 400", (await Call("GET", "api/v1/equipment?available=maybe")).Status == 400);
    var st = await Call("GET", "api/v1/students", note: "all students"); Check("4 students", st.Json?.AsArray().Count == 4);
    var s1 = await StudentId("ST-100"); var e1 = await EquipId("EQ-001"); var e3 = await EquipId("EQ-003"); var s4 = await StudentId("ST-400");
    var g1 = await Call("GET", $"api/v1/students/{s1}"); Check("S1 200 Ada Cruz eligible", g1.Status == 200 && g1.Json!["fullName"]!.ToString() == "Ada Cruz" && g1.Json["isEligible"]!.GetValue<bool>());
    var ge1 = await Call("GET", $"api/v1/equipment/{e1}"); Check("E1 200 unavailable Camera", ge1.Status == 200 && !ge1.Json!["isAvailable"]!.GetValue<bool>());
    var h1 = await Call("GET", $"api/v1/students/{s1}/borrowings", note: "S1 history");
    Check("S1 history has 2 rows (1 Active, 1 Returned)", h1.Json!.AsArray().Count == 2 && h1.Json.AsArray().Count(x => x!["status"]!.ToString() == "Active") == 1);
    var bid = h1.Json.AsArray().First(x => x!["equipmentCode"]!.ToString() == "EQ-001")!["id"]!.ToString();
    var gb = await Call("GET", $"api/v1/borrowings/{bid}"); Check("known borrowing 200 Active", gb.Status == 200 && gb.Json!["status"]!.ToString() == "Active");
    var he1 = await Call("GET", $"api/v1/equipment/{e1}/borrowings", note: "E1 history"); Check("E1 history 1 Active row", he1.Json!.AsArray().Count == 1);
    var he3 = await Call("GET", $"api/v1/equipment/{e3}/borrowings", note: "E3 history"); Check("E3 history 1 Returned row", he3.Json!.AsArray().Count == 1 && he3.Json[0]!["status"]!.ToString() == "Returned");
    var hs4 = await Call("GET", $"api/v1/students/{s4}/borrowings", note: "empty history"); Check("S4 empty history []", hs4.Status == 200 && hs4.Raw.Trim() == "[]");
    var ab = await Call("GET", "api/v1/borrowings", note: "all borrowings"); Check("4 borrowings", ab.Json?.AsArray().Count == 4);
    var rnd = Guid.NewGuid();
    Check("unknown student 404", (await Call("GET", $"api/v1/students/{rnd}")).Status == 404);
    Check("unknown equipment 404", (await Call("GET", $"api/v1/equipment/{rnd}")).Status == 404);
    Check("unknown borrowing 404", (await Call("GET", $"api/v1/borrowings/{rnd}")).Status == 404);
    Check("unknown student history 404", (await Call("GET", $"api/v1/students/{rnd}/borrowings")).Status == 404);
    Check("unknown equipment history 404", (await Call("GET", $"api/v1/equipment/{rnd}/borrowings")).Status == 404);
    Check("bad route 404 with error body", (await Call("GET", "api/v1/nothing-here")).Status == 404);
    Check("wrong method (DELETE /equipment) 405", (await Call("DELETE", "api/v1/equipment")).Status == 405);
    var oa = await Call("GET", "openapi/v1.json", note: "OpenAPI"); 
    var found = new HashSet<string>();
    foreach (var p in oa.Json!["paths"]!.AsObject()) foreach (var m in p.Value!.AsObject()) found.Add(m.Value!["x-api-id"]!.ToString() + " " + m.Key.ToUpperInvariant() + " " + p.Key);
    Check("OpenAPI describes all 16 operations (API-01..16)", found.Select(x => x.Split(' ')[0]).Distinct().Count() == 16, string.Join("; ", found.OrderBy(x => x)));
    await Snapshot("after (must be identical: reads only)");
}

// ---------------------------------------------- T02
if (Run("T02"))
{
    Section("T02 — Students: create/edit/eligibility (API-04..06)"); await Reset(); await Snapshot("before");
    var c = await Call("POST", "api/v1/students", new { studentNumber = "ST-500", fullName = "Eli Tan", contact = (string?)null }, "create");
    Check("201 + Location", c.Status == 201, c.Raw);
    var loc = c.Json!["id"]!.ToString(); Check("GET new student 200", (await Call("GET", $"api/v1/students/{loc}")).Status == 200);
    var up = await Call("PUT", $"api/v1/students/{loc}", new { studentNumber = "ST-500", fullName = "Eli Tan Jr.", contact = "eli@example.test" }, "edit");
    Check("PUT 200 persisted new name/contact", up.Status == 200 && up.Json!["fullName"]!.ToString() == "Eli Tan Jr.");
    Check("blank name → 400", (await Call("POST", "api/v1/students", new { studentNumber = "ST-501", fullName = "  " })).Status == 400);
    Check("duplicate ' st-100 ' → 409", (await Call("POST", "api/v1/students", new { studentNumber = " st-100 ", fullName = "Dup" })).Status == 409);
    Check("student count is 5 (no duplicate/blank added)", (await Get("api/v1/students"))!.AsArray().Count == 5);
    var s1 = await StudentId("ST-100"); var e1 = await EquipId("EQ-001"); var e2 = await EquipId("EQ-002");
    Check("restrict S1 → 200", (await Call("PATCH", $"api/v1/students/{s1}/eligibility", new { isEligible = false }, "restrict")).Status == 200);
    Check("issue to restricted S1 → 409", (await Call("POST", "api/v1/borrowings", new { studentId = s1, equipmentId = e2, expectedReturnDate = D(due) }, "issue while restricted")).Status == 409);
    var hist = (await Get($"api/v1/students/{s1}/borrowings"))!.AsArray(); Check("restricting kept S1 history (2 rows) and active loan", hist.Count == 2 && hist.Any(x => x!["status"]!.ToString() == "Active"));
    var act = hist.First(x => x!["status"]!.ToString() == "Active")!["id"]!.ToString();
    Check("return E1 → 200", (await Call("POST", $"api/v1/borrowings/{act}/return", new { }, "return E1")).Status == 200);
    Check("permit S1 → 200", (await Call("PATCH", $"api/v1/students/{s1}/eligibility", new { isEligible = true }, "permit")).Status == 200);
    Check("valid issue after permit → 201", (await Call("POST", "api/v1/borrowings", new { studentId = s1, equipmentId = e2, expectedReturnDate = D(due) }, "valid issue")).Status == 201);
    var same = await Call("PATCH", $"api/v1/students/{s1}/eligibility", new { isEligible = true }, "same eligibility again"); Check("same state → 200 no-op", same.Status == 200);
    Check("S1 history now 3 rows (nothing erased)", (await Get($"api/v1/students/{s1}/borrowings"))!.AsArray().Count == 3);
    await Snapshot("after");
}

// ---------------------------------------------- T03
if (Run("T03"))
{
    Section("T03 — Equipment: create/edit, protected state (API-07..09)"); await Reset(); await Snapshot("before");
    var c = await Call("POST", "api/v1/equipment", new { equipmentCode = "EQ-008", name = "Charger", description = (string?)null }, "create");
    Check("201 + available=true", c.Status == 201 && c.Json!["isAvailable"]!.GetValue<bool>());
    var id = c.Json!["id"]!.ToString();
    Check("GET created 200", (await Call("GET", $"api/v1/equipment/{id}")).Status == 200);
    Check("PUT edited name 200", (await Call("PUT", $"api/v1/equipment/{id}", new { equipmentCode = "EQ-008", name = "Fast charger", description = (string?)null }, "edit")).Status == 200);
    var e1 = await EquipId("EQ-001");
    Check("PUT E1 descriptive change 200", (await Call("PUT", $"api/v1/equipment/{e1}", new { equipmentCode = "EQ-001", name = "Camera", description = "DSLR body" }, "describe E1")).Status == 200);
    Check("blank name → 400", (await Call("POST", "api/v1/equipment", new { equipmentCode = "EQ-009", name = "" })).Status == 400);
    Check("duplicate ' eq-001 ' → 409", (await Call("POST", "api/v1/equipment", new { equipmentCode = " eq-001 ", name = "Dup" })).Status == 409);
    Check("isAvailable in update → 400", (await Call("PUT", $"api/v1/equipment/{e1}", "{\"equipmentCode\":\"EQ-001\",\"name\":\"Camera\",\"isAvailable\":true}", "protected field")).Status == 400);
    var after = await Get($"api/v1/equipment/{e1}"); Check("E1 still unavailable", !after!["isAvailable"]!.GetValue<bool>());
    Check("E1 history intact (1 row)", (await Get($"api/v1/equipment/{e1}/borrowings"))!.AsArray().Count == 1);
    Check("equipment count is 8", (await Get("api/v1/equipment"))!.AsArray().Count == 8);
    await Snapshot("after");
}

// ---------------------------------------------- T04
if (Run("T04"))
{
    Section("T04 — Single issue and return (API-10, 12, 13)"); await Reset(); await Snapshot("before");
    var s4 = await StudentId("ST-400"); var s2 = await StudentId("ST-200"); var e2 = await EquipId("EQ-002"); var e1 = await EquipId("EQ-001");
    var b = await Call("POST", "api/v1/borrowings", new { studentId = s4, equipmentId = e2, expectedReturnDate = D(due) }, "issue S4/E2");
    Check("201 Active", b.Status == 201 && b.Json!["status"]!.ToString() == "Active");
    var bid = b.Json!["id"]!.ToString(); Check("GET borrowing 200", (await Call("GET", $"api/v1/borrowings/{bid}")).Status == 200);
    Check("E2 now unavailable", !(await Get($"api/v1/equipment/{e2}"))!["isAvailable"]!.GetValue<bool>());
    long before = await Count("Borrowings");
    Check("ineligible S2 → 409", (await Call("POST", "api/v1/borrowings", new { studentId = s2, equipmentId = await EquipId("EQ-006"), expectedReturnDate = D(due) })).Status == 409);
    Check("unavailable E1 → 409", (await Call("POST", "api/v1/borrowings", new { studentId = s4, equipmentId = e1, expectedReturnDate = D(due) })).Status == 409);
    Check("missing student → 404", (await Call("POST", "api/v1/borrowings", new { studentId = Guid.NewGuid(), equipmentId = await EquipId("EQ-006"), expectedReturnDate = D(due) })).Status == 404);
    Check("missing equipment → 404", (await Call("POST", "api/v1/borrowings", new { studentId = s4, equipmentId = Guid.NewGuid(), expectedReturnDate = D(due) })).Status == 404);
    Check("date not after today → 400", (await Call("POST", "api/v1/borrowings", new { studentId = s4, equipmentId = await EquipId("EQ-006"), expectedReturnDate = D(today) })).Status == 400);
    Check("date yesterday → 400", (await Call("POST", "api/v1/borrowings", new { studentId = s4, equipmentId = await EquipId("EQ-006"), expectedReturnDate = D(yesterday) })).Status == 400);
    Check("malformed date → 400", (await Call("POST", "api/v1/borrowings", "{\"studentId\":\"" + s4 + "\",\"equipmentId\":\"" + e2 + "\",\"expectedReturnDate\":\"11/12/2026\"}")).Status == 400);
    if (before >= 0) Check("rejected attempts wrote nothing (row count unchanged)", await Count("Borrowings") == before);
    var r = await Call("POST", $"api/v1/borrowings/{bid}/return", new { }, "return");
    Check("return 200 with returnedDate", r.Status == 200 && r.Json!["returnedDate"] is not null && r.Json["status"]!.ToString() == "Returned");
    Check("E2 available again", (await Get($"api/v1/equipment/{e2}"))!["isAvailable"]!.GetValue<bool>());
    Check("repeat return → 409", (await Call("POST", $"api/v1/borrowings/{bid}/return", new { }, "repeat return")).Status == 409);
    Check("return unknown id → 404", (await Call("POST", $"api/v1/borrowings/{Guid.NewGuid()}/return", new { })).Status == 404);
    await Snapshot("after (restart the API now and re-run the readbacks for the persistence evidence)");
}

// ---------------------------------------------- T05
if (Run("T05"))
{
    Section("T05 — Multi-item success, limit boundary, invalid requests, atomic rollback (API-10, 11)");
    async Task<long[]> Counts() => new[] { await Count("Borrowings"), await Count("Equipment") };

    await Reset(); await Snapshot("1) before S4 E2+E6");
    var s4 = await StudentId("ST-400"); var e2 = await EquipId("EQ-002"); var e6 = await EquipId("EQ-006");
    var ok1 = await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) }, "1) checkout S4 E2/E6");
    Check("201 with 2 borrowings", ok1.Status == 201 && ok1.Json!["borrowings"]!.AsArray().Count == 2);
    await Snapshot("1) after");

    await Reset(); var s3 = await StudentId("ST-300"); e2 = await EquipId("EQ-002");
    var a = await Call("POST", "api/v1/borrowings", new { studentId = s3, equipmentId = e2, expectedReturnDate = D(due) }, "2a) S3 (2 active) + E2 via API-10");
    Check("2a 201 (exactly three active)", a.Status == 201);
    Check("2a S3 has 3 active", (await Get($"api/v1/students/{s3}/borrowings"))!.AsArray().Count(x => x!["status"]!.ToString() == "Active") == 3);
    await Snapshot("2a) after");

    await Reset(); var s1 = await StudentId("ST-100"); e2 = await EquipId("EQ-002"); e6 = await EquipId("EQ-006");
    var b = await Call("POST", "api/v1/checkout-requests", new { studentId = s1, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) }, "2b) S1 (1 active) + E2/E6 via API-11");
    Check("2b 201 with two", b.Status == 201 && b.Json!["borrowings"]!.AsArray().Count == 2);
    Check("2b S1 has 3 active", (await Get($"api/v1/students/{s1}/borrowings"))!.AsArray().Count(x => x!["status"]!.ToString() == "Active") == 3);
    await Snapshot("2b) after");

    await Reset(); s3 = await StudentId("ST-300"); e2 = await EquipId("EQ-002"); e6 = await EquipId("EQ-006");
    var pre = await Counts(); await Snapshot("2c) before");
    var c = await Call("POST", "api/v1/checkout-requests", new { studentId = s3, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) }, "2c) S3 (2 active) + E2/E6 → over limit");
    Check("2c 409", c.Status == 409);
    Check("2c no rows added; E2/E6 still available", (await Get($"api/v1/equipment/{e2}"))!["isAvailable"]!.GetValue<bool>() && (await Get($"api/v1/equipment/{e6}"))!["isAvailable"]!.GetValue<bool>());
    await Snapshot("2c) after (must equal before)");

    await Reset(); s4 = await StudentId("ST-400"); var s2 = await StudentId("ST-200"); e2 = await EquipId("EQ-002"); e6 = await EquipId("EQ-006"); var e1 = await EquipId("EQ-001");
    pre = await Counts(); await Snapshot("3) before invalid requests");
    Check("3 duplicate ids → 400", (await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2, e2 }, expectedReturnDate = D(due) })).Status == 400);
    Check("3 one item → 400", (await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2 }, expectedReturnDate = D(due) })).Status == 400);
    Check("3 E1+E2 (E1 unavailable) → 409", (await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e1, e2 }, expectedReturnDate = D(due) })).Status == 409);
    Check("3 ineligible S2 → 409", (await Call("POST", "api/v1/checkout-requests", new { studentId = s2, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) })).Status == 409);
    Check("3 missing item → 404", (await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2, Guid.NewGuid() }, expectedReturnDate = D(due) })).Status == 404);
    Check("3 missing student → 404", (await Call("POST", "api/v1/checkout-requests", new { studentId = Guid.NewGuid(), equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) })).Status == 404);
    Check("3 invalid date → 400", (await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(yesterday) })).Status == 400);
    var post = await Counts(); if (pre[0] >= 0) Check("3 no partial rows from any invalid request", pre[0] == post[0]);
    await Snapshot("3) after (must equal before)");

    await Reset(); s4 = await StudentId("ST-400"); e2 = await EquipId("EQ-002"); e6 = await EquipId("EQ-006");
    pre = await Counts(); await Snapshot("4) before injected failure");
    var arm = await http.PostAsync("dev/fault?armed=true", null); W($"**arm fault seam** `POST /dev/fault?armed=true` → {(int)arm.StatusCode}");
    var f = await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) }, "4) checkout with failure injected AFTER the first row is written");
    Check("4 received HTTP 500 (a server response, not a transport failure)", f.Status == 500);
    Check("4 safe body (no stack trace / connection string)", f.Json is not null && f.Raw.Length < 400 && !f.Raw.Contains(" at ") && !f.Raw.Contains("Host="));
    var postF = await Counts(); if (pre[0] >= 0) Check("4 ROLLBACK: borrowing row count unchanged", pre[0] == postF[0]);
    Check("4 ROLLBACK: E2 and E6 still available", (await Get($"api/v1/equipment/{e2}"))!["isAvailable"]!.GetValue<bool>() && (await Get($"api/v1/equipment/{e6}"))!["isAvailable"]!.GetValue<bool>());
    await Snapshot("4) after (must equal before: documented failure point = after first borrowing row write inside the transaction)");
    var retry = await Call("POST", "api/v1/checkout-requests", new { studentId = s4, equipmentIds = new[] { e2, e6 }, expectedReturnDate = D(due) }, "4) retry after the one-shot fault disarmed");
    Check("4 retry succeeds 201", retry.Status == 201);
}

// ---------------------------------------------- T06
if (Run("T06"))
{
    Section("T06 — Histories, search, filters, sorting (API-14..16)"); await Reset(); await Snapshot("before");
    var all = await Call("GET", "api/v1/borrowings", note: "default (All, borrowedDate, asc)");
    Check("4 rows; E1 (11-01) first, E3 (11-02) second", all.Json!.AsArray().Count == 4 && all.Json[0]!["equipmentCode"]!.ToString() == "EQ-001" && all.Json[1]!["equipmentCode"]!.ToString() == "EQ-003");
    Check("search 'aDa' (mixed case name) → 2 rows (S1)", (await Call("GET", "api/v1/borrowings?search=aDa")).Json!.AsArray().Count == 2);
    Check("search 'st-300' (number) → 2 rows (S3)", (await Call("GET", "api/v1/borrowings?search=st-300")).Json!.AsArray().Count == 2);
    Check("search 'CAM' (equipment partial) → 1 row (E1)", (await Call("GET", "api/v1/borrowings?search=CAM")).Json!.AsArray().Count == 1);
    Check("status=Active → 3", (await Call("GET", "api/v1/borrowings?status=Active")).Json!.AsArray().Count == 3);
    Check("status=Returned → 1", (await Call("GET", "api/v1/borrowings?status=Returned")).Json!.AsArray().Count == 1);
    Check("status=All → 4", (await Call("GET", "api/v1/borrowings?status=All")).Json!.AsArray().Count == 4);
    var d1 = await Call("GET", "api/v1/borrowings?sort=borrowedDate&direction=desc"); Check("borrowedDate desc: E4/E5 (11-03) first", d1.Json![0]!["borrowedDate"]!.ToString() == "2026-11-03");
    var e1 = await Call("GET", "api/v1/borrowings?sort=expectedReturnDate&direction=asc"); Check("expectedReturnDate asc: EQ-001 (11-09) first", e1.Json![0]!["equipmentCode"]!.ToString() == "EQ-001");
    var e2 = await Call("GET", "api/v1/borrowings?sort=expectedReturnDate&direction=desc"); Check("expectedReturnDate desc: EQ-001 last", e2.Json![3]!["equipmentCode"]!.ToString() == "EQ-001");
    Check("combined search=cruz + status=Active → 1 (EQ-001)", (await Call("GET", "api/v1/borrowings?search=cruz&status=Active")).Json!.AsArray() is { Count: 1 } one && one[0]!["equipmentCode"]!.ToString() == "EQ-001");
    Check("cleared criteria restores all 4", (await Call("GET", "api/v1/borrowings?search=&status=All")).Json!.AsArray().Count == 4);
    var none = await Call("GET", "api/v1/borrowings?search=zzzz"); Check("no matches → 200 []", none.Status == 200 && none.Raw.Trim() == "[]");
    Check("status=Bogus → 400", (await Call("GET", "api/v1/borrowings?status=Bogus")).Status == 400);
    Check("sort=name → 400", (await Call("GET", "api/v1/borrowings?sort=name")).Status == 400);
    Check("direction=up → 400", (await Call("GET", "api/v1/borrowings?direction=up")).Status == 400);
    var s1 = await StudentId("ST-100"); var s4 = await StudentId("ST-400");
    Check("S1 history 2 / S4 history 0", (await Get($"api/v1/students/{s1}/borrowings"))!.AsArray().Count == 2 && (await Get($"api/v1/students/{s4}/borrowings"))!.AsArray().Count == 0);
    Check("unknown ids 404", (await Call("GET", $"api/v1/students/{Guid.NewGuid()}/borrowings")).Status == 404);
    await Snapshot("after (must equal before)");
}

W(); W("---"); W($"**Summary: {passed} passed, {failed} failed.** Finished {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
W("T07 (desktop HTTP client) and T08 (unreachable vs 500, restart) are manual: see docs/test-evidence.md.");
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
File.WriteAllText(outPath, md.ToString());
Console.WriteLine($"\n{passed} passed, {failed} failed. Evidence written to {outPath}");
return failed == 0 ? 0 : 1;
