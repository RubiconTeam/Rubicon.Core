using System.Collections.Generic;
using System.Linq;

namespace Rubicon.Core.Chart;

[GlobalClass] public partial class SectionData : Resource
{
    [Export] public int Measure = 0;

    [Export] public RowData[] Rows = [];

    public void ConvertData(BpmInfo[] bpmInfo, SvChange[] svChanges)
    {
        for (int r = 0; r < Rows.Length; r++)
        {
            RowData row = Rows[r];
            row.Section = this;
            row.ConvertData(bpmInfo, svChanges);
        }
    }

    public RowData AddRow(byte offset, byte quant)
    {
        FixOffsetAndQuant(ref offset, ref quant);
        
        RowData row = GetRow(offset, quant);
        if (row != null)
            return row;

        row = new RowData();
        row.Section = this;
        row.Offset = offset;
        row.Quant = quant;
        
        List<RowData> rowList = Rows.ToList();
        rowList.Add(row);
        Rows = rowList.ToArray();
        
        SortRows();
        return row;
    }

    public void RemoveRow(RowData row)
    {
        List<RowData> rowList = Rows.ToList();
        rowList.Remove(row);
        Rows = rowList.ToArray();
        
        SortRows();
    }

    public void RemoveRowAt(byte offset, byte quant)
    {
        FixOffsetAndQuant(ref offset, ref quant);
        RowData row = GetRow(offset, quant);
        if (row == null)
            return;
        
        RemoveRow(row);
    }

    public void CleanupRows()
    {
        List<RowData> rowList = Rows.ToList();
        for (int i = 0; i < Rows.Length; i++)
        {
            RowData row = Rows[i];
            if (row.StartNotes.Length > 0 || row.EndNotes.Length > 0)
                continue;

            rowList.Remove(row);
        }
        
        Rows = rowList.ToArray();
        SortRows();
    }
    
    public RowData GetRow(byte offset, byte quant)
    {
        FixOffsetAndQuant(ref offset, ref quant);
        return Rows.FirstOrDefault(x => x.Offset == offset && x.Quant == quant);
    }
    
    public bool HasRow(byte offset, byte quant)
    {
        FixOffsetAndQuant(ref offset, ref quant);
        return Rows.Any(x => x.Offset == offset && x.Quant == quant);
    }

    public void SortRows()
    {
        Array.Sort(Rows, (x, y) => ((float)x.Offset / x.Quant).CompareTo((float)y.Offset / y.Quant));
    }

    private void FixOffsetAndQuant(ref byte offset, ref byte quant)
    {
        byte[] quants = RubiChartConstants.Quants;
        for (int q = 0; q < quants.Length; q++)
        {
            byte curQuant = quants[q];
            if (curQuant >= quant)
                break;

            bool isOffsetDivisible = offset % curQuant == 0;
            bool isQuantDivisible = quant % curQuant == 0;
            
            if (!isOffsetDivisible || !isQuantDivisible)
                continue;
            
            offset /= curQuant;
            quant = curQuant;
            break;
        }
    }
}