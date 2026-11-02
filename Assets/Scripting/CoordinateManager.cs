using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// Bulle v1.0: original CSV format and public methods preserved.
public class CoordinateManager : MonoBehaviour
{
    [Serializable]
    public class ObjectEntry
    {
        public string label;
        public Transform target;
    }

    [Header("20 points : 4 zones x 5 points")]
    [SerializeField] private List<ObjectEntry> objects = new List<ObjectEntry>();
    [SerializeField] private bool requireExactly20Points = true;

    [Serializable]
    public class CoordinateRecord
    {
        public int index;
        public string objectName;
        public Vector3 position;
    }

    private readonly List<CoordinateRecord> records = new List<CoordinateRecord>();
    public int ObjectCount => objects.Count;
    public string LastSavedPath { get; private set; }

    public event Action<string> SaveSucceeded;
    public event Action<string> SaveFailed;

    private bool TryCapture(out string error)
    {
        records.Clear();
        error = null;

        if (requireExactly20Points && objects.Count != 20)
        {
            error = "Expected 20 points, found " + objects.Count;
            return false;
        }

        for (int i = 0; i < objects.Count; i++)
        {
            ObjectEntry entry = objects[i];
            if (entry == null || entry.target == null)
            {
                error = "Missing control point at index " + i;
                records.Clear();
                return false;
            }

            Vector3 p = entry.target.position;
            if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) ||
                float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z))
            {
                error = "Invalid coordinates at index " + i;
                records.Clear();
                return false;
            }

            records.Add(new CoordinateRecord
            {
                index = i,
                objectName = string.IsNullOrWhiteSpace(entry.label) ? entry.target.name : entry.label,
                position = p
            });
        }
        return true;
    }

    private void NotifyFailure(string reason)
    {
        Debug.LogError("[Bulle CSV] " + reason);
        SaveFailed?.Invoke(reason);
    }

    // Kept for existing UnityEvents or other scripts.
    public void CaptureAllCoordinates()
    {
        if (!TryCapture(out string error))
            Debug.LogWarning("[Bulle CSV] " + error);
    }

    // Exports the last captured snapshot, as in the original script.
    public void ExportToCSV()
    {
        if (records.Count == 0 || (requireExactly20Points && records.Count != 20))
        {
            NotifyFailure("No valid complete capture available.");
            return;
        }

        try
        {
            // Original location preserved: Documents/RelevesVR.
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "RelevesVR");
            Directory.CreateDirectory(folder);

            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture);
            string path = Path.Combine(folder, "Coordonnees_" + stamp + ".csv");
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Index;Nom;X;Y;Z");

            foreach (CoordinateRecord r in records)
            {
                Vector3 p = r.position;
                string safeName = "\"" + (r.objectName ?? "").Replace("\"", "\"\"") + "\"";
                csv.Append(r.index).Append(';').Append(safeName).Append(';')
                    .Append(p.x.ToString("R", CultureInfo.InvariantCulture)).Append(';')
                    .Append(p.y.ToString("R", CultureInfo.InvariantCulture)).Append(';')
                    .Append(p.z.ToString("R", CultureInfo.InvariantCulture)).AppendLine();
            }

            // Exactly one UTF-8 BOM: do not prepend "\\uFEFF" manually.
            File.WriteAllText(path, csv.ToString(), new UTF8Encoding(true));
            LastSavedPath = path;
            Debug.Log("[Bulle CSV] Saved " + records.Count + " points: " + path);
            SaveSucceeded?.Invoke(path);
        }
        catch (Exception ex)
        {
            NotifyFailure(ex.Message);
        }
    }

    // Existing right-trigger acquisition script can keep calling this method.
    public void CaptureAllAndExport()
    {
        if (!TryCapture(out string error))
        {
            NotifyFailure(error);
            return;
        }
        ExportToCSV();
    }
}
