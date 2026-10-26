using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class CoordinateManager : MonoBehaviour
{
    [Serializable]
    public class ObjectEntry
    {
        public string label;
        public Transform target;
    }

    [Header("Liste des objets ?relever")]
    [SerializeField]
    private List<ObjectEntry> objects = new List<ObjectEntry>();

    [Serializable]
    public class CoordinateRecord
    {
        public int index;
        public string objectName;
        public Vector3 position;
    }

    private readonly List<CoordinateRecord> records =
        new List<CoordinateRecord>();

    public int ObjectCount => objects.Count;

    // ?appeler depuis un seul bouton.
    public void CaptureAllCoordinates()
    {
        records.Clear();

        for (int i = 0; i < objects.Count; i++)
        {
            ObjectEntry entry = objects[i];

            if (entry == null || entry.target == null)
            {
                Debug.LogWarning(
                    $"Objet manquant ?l'index {i}. Entr閑 ignor閑."
                );
                continue;
            }

            Vector3 pos = entry.target.position;

            string objectName = string.IsNullOrWhiteSpace(entry.label)
                ? entry.target.name
                : entry.label;

            records.Add(new CoordinateRecord
            {
                index = i,
                objectName = objectName,
                position = pos
            });

            Debug.Log(
                $"Objet {i} ({objectName}) : " +
                $"X={pos.x}, Y={pos.y}, Z={pos.z}"
            );
        }

        Debug.Log(
            $"Acquisition termin閑 : {records.Count} objet(s) relev?s) " +
            $"sur {objects.Count} configur?s)."
        );
    }

    // Export des derni鑢es coordonn閑s acquises.

    public void ExportToCSV()
    {
        if (records.Count == 0)
        {
            Debug.LogWarning(
                "Aucune coordonn閑 disponible. Lance d'abord l'acquisition."
            );
            return;
        }

        // Dossier de destination
        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RelevesVR"
        );

        Directory.CreateDirectory(folderPath);

        // Date et heure pour cr閑r un nom unique
        string timestamp = DateTime.Now.ToString(
            "yyyy-MM-dd_HH-mm-ss-fff",
            CultureInfo.InvariantCulture
        );

        string fileName = $"Coordonnees_{timestamp}.csv";
        string filePath = Path.Combine(folderPath, fileName);

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Index;Nom;X;Y;Z");

        foreach (CoordinateRecord record in records)
        {
            Vector3 p = record.position;

            string safeName = "\"" +
                record.objectName.Replace("\"", "\"\"") +
                "\"";

            csv.AppendLine(
                $"{record.index};{safeName};" +
                $"{p.x.ToString(CultureInfo.InvariantCulture)};" +
                $"{p.y.ToString(CultureInfo.InvariantCulture)};" +
                $"{p.z.ToString(CultureInfo.InvariantCulture)}"
            );
        }

        File.WriteAllText(
            filePath,
            "\uFEFF" + csv.ToString(),
            Encoding.UTF8
        );

        Debug.Log($"Export CSV termin?: {filePath}");
    }


    // Variante pratique : acquisition puis export avec un seul clic.
    public void CaptureAllAndExport()
    {
        CaptureAllCoordinates();
        ExportToCSV();
    }
}
