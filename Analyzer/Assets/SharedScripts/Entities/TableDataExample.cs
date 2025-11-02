using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Numerics;
using static DataTableUtility;
using Assets.DataTable2col;

[Serializable]
[Tablizable(typeof(TableDataExample))]
public class TableDataExample {
    public int Intfield;
    public string Stringfield;
    public DateTime DateTimeField;
    public List<Vector2> V2ListField;
    public int IntProp { get; private set; }
    public string StringProp { get; private set; }
    public DateTime DateTimeProp { get; private set; }
    public List<Vector2> V2ListProp { get; private set; }
    public int IntPropShell {
        get => Intfield;
        set => Intfield = value;
    }
}