public static partial class EntityExtensions {
    public static bool Save(this Snapshot snapshot, string name = null) => SLUtility.Save(snapshot, name);
    //寫這種Extension不就增加耦合
}