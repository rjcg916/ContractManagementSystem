using System.Xml;
using System;
using System.IO;
using System.Text;
public class InfopathFormGrocker
{
    private readonly bool RemovePIs;

    public InfopathFormGrocker(bool removePIs)
    {
        RemovePIs = removePIs;
    }

    public XmlDocument ComponentContent { get; private set; }

    public bool ExtractComponent(Stream formStream, string componentFilename)
    {
        if (formStream == null) throw new ArgumentNullException("formStream");
        if (string.IsNullOrEmpty(componentFilename)) throw new ArgumentNullException("componentFilename");

        ComponentContent = null;

        // reset the stream
        formStream.Seek(0, SeekOrigin.Begin);

        // do the extraction
        var cabExtractor = new CabLib.Extract();
        cabExtractor.SetSingleFile(componentFilename);
        cabExtractor.evAfterCopyFile += OnAfterCopyFile;
        cabExtractor.ExtractStream(formStream, "MEMORY");

        return ComponentContent != null;
    }

    private void OnAfterCopyFile(string fileName, byte[] u8FileContent)
    {
        var content = Encoding.ASCII.GetString(u8FileContent);

        // remove Unicode BOM if present
        if (content.Length > 1 && content[0] == 0xFEFF)
            content = content.Substring(1);

        // load into xmldoc
        ComponentContent = new XmlDocument();
        ComponentContent.LoadXml(content);

        if (!RemovePIs) return;

        // remove PIs
        var piNodes = ComponentContent.SelectNodes("/processing-instruction()");
        if (piNodes == null) return;
        foreach (XmlNode piNode in piNodes)
        {
            if (piNode.LocalName == "mso-infoPathSolution" || piNode.LocalName == "mso-application")
                ComponentContent.RemoveChild(piNode);
        }
    }
}
