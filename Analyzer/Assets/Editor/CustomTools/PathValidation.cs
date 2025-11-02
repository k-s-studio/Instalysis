using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class PathValidation {
    [MenuItem("CustomTools/PathValidation/FileNameCheck")]
    public static void FileNameCheck() {
        string filename = "000000.ss";
        Debug.Log(filename + (filename == Path.GetFileName(filename) ? " = " : " ≠ ") + Path.GetFileName(filename));
        Debug.Log(filename + (Path.GetInvalidFileNameChars().Any(c => filename.Contains(c)) ? " has " : " has no ") + "invalid chars.");
    }

    [MenuItem("CustomTools/PathValidation/ValidDirectoryCheck")]
    public static void ValidDirectoryCheck() {
        string dir = App.DATAPATH;
        Debug.Log(dir + (dir == Path.GetDirectoryName(dir) ? " = " : " ≠ ") + Path.GetDirectoryName(dir));
        Debug.Log(dir + (Path.GetInvalidPathChars().Any(c => dir.Contains(c)) ? " has " : " has no ") + "invalid path chars.");
    }

    [MenuItem("CustomTools/PathValidation/InvalidPathChars")]
    public static void InvalidPathChars() =>
        Debug.Log($"{Path.GetInvalidPathChars().Length} Invalid Path Chars: " + new string(Path.GetInvalidPathChars()));

    [MenuItem("CustomTools/PathValidation/InvalidFileNameChars")]
    public static void InvalidFileNameChars() =>
        Debug.Log($"{Path.GetInvalidFileNameChars().Length} Invalid FileName Chars: " + new string(Path.GetInvalidFileNameChars()));
}