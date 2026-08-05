namespace Tools.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Path"></param>
    /// <param name="groupSubfolders">if true -> all subfolders will be explored and items will be placed in same group. Otherwise subfolders ignored</param>
    internal record InicializationData(
        string Name,
        string Path,
        bool groupSubfolders = true
    );
}
