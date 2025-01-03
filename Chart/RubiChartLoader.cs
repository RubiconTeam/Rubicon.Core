using System.Linq;
using System.Text;
using Range = System.Range;

namespace Rubicon.Core.Chart;

public static class RubiChartLoader
{
    public static void SetupFromVersionOne(this RubiChart chart, byte[] bytes)
    {
        int offset = 0;
        
        // Version is most likely already set
        chart.Difficulty = BitConverter.ToUInt32(bytes.Take(new Range(4, 8)).ToArray());
        chart.ScrollSpeed = BitConverter.ToSingle(bytes.Take(new Range(8, 12)).ToArray());
        
        int charterLength = BitConverter.ToInt32(bytes.Take(new Range(12, 16)).ToArray());
        offset = 16 + charterLength;
        chart.Charter = Encoding.UTF8.GetString(bytes.Take(new Range(16, offset)).ToArray());
        
        int typeArrayLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
        offset += 4;
        string[] noteTypes = new string[typeArrayLength];
        for (int i = 0; i < typeArrayLength; i++)
        {
            int typeLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            
            string noteType = Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + typeLength)).ToArray());
            offset += typeLength;
            
            noteTypes[i] = noteType;
        }
        
        int chartsLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
        offset += 4;
        chart.Charts = new IndividualChart[chartsLength];
        for (int i = 0; i < chartsLength; i++)
        {
            IndividualChart indChart = new IndividualChart();

            int nameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.Name = Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + nameLength)).ToArray());
            offset += nameLength;

            indChart.Lanes = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;

            int switchesLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.Switches = new TargetSwitch[switchesLength];
            for (int j = 0; j < switchesLength; j++)
            {
                TargetSwitch @switch = new TargetSwitch();

                @switch.Time = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                offset += 4;

                int lineNameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;
                @switch.Name =
                    Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + lineNameLength)).ToArray());
                offset += lineNameLength;
                
                indChart.Switches[j] = @switch;
            }

            int svChangeLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.SvChanges = new SvChange[svChangeLength];
            for (int j = 0; j < svChangeLength; j++)
            {
                SvChange svChange = new SvChange();

                svChange.Time = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                offset += 4;

                svChange.Multiplier = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;

                indChart.SvChanges[j] = svChange;
            }

            int notesLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.Notes = new NoteData[notesLength];
            for (int j = 0; j < notesLength; j++)
            {
                NoteData noteData = new NoteData();

                noteData.Time = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;

                noteData.Length = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;

                noteData.Lane = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;

                noteData.Type = noteTypes[BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray())];
                offset += 4;

                int paramCount = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;
                for (int k = 0; k < paramCount; k++)
                {
                    int pNameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                    offset += 4;

                    StringName pName =
                        Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + pNameLength)).ToArray());
                    offset += pNameLength;

                    int pValueLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                    offset += 4;

                    Variant pValue = GD.BytesToVar(bytes.Take(new Range(offset, offset + pValueLength)).ToArray());
                    offset += pValueLength;

                    noteData.Parameters.Add(pName, pValue);
                }

                indChart.Notes[j] = noteData;
            }

            chart.Charts[i] = indChart;
        }
    }

    public static void SetupFromVersionOneM1(this RubiChart chart, byte[] bytes)
    {
        int offset = 0;
        
        // Version is most likely already set
        chart.Difficulty = BitConverter.ToUInt32(bytes.Take(new Range(4, 8)).ToArray());
        chart.ScrollSpeed = BitConverter.ToSingle(bytes.Take(new Range(8, 12)).ToArray());
        
        int charterLength = BitConverter.ToInt32(bytes.Take(new Range(12, 16)).ToArray());
        offset = 16 + charterLength;
        chart.Charter = Encoding.UTF8.GetString(bytes.Take(new Range(16, offset)).ToArray());
        
        int typeArrayLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
        offset += 4;
        string[] noteTypes = new string[typeArrayLength];
        for (int i = 0; i < typeArrayLength; i++)
        {
            int typeLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            
            string noteType = Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + typeLength)).ToArray());
            offset += typeLength;
            
            noteTypes[i] = noteType;
        }
        
        int chartsLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
        offset += 4;
        chart.Charts = new IndividualChart[chartsLength];
        for (int i = 0; i < chartsLength; i++)
        {
            IndividualChart indChart = new IndividualChart();

            int nameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.Name = Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + nameLength)).ToArray());
            offset += nameLength;

            indChart.Lanes = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;

            int switchesLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.Switches = new TargetSwitch[switchesLength];
            for (int j = 0; j < switchesLength; j++)
            {
                TargetSwitch @switch = new TargetSwitch();

                @switch.Time = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                offset += 4;

                int lineNameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;
                @switch.Name =
                    Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + lineNameLength)).ToArray());
                offset += lineNameLength;
                
                indChart.Switches[j] = @switch;
            }

            int svChangeLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.SvChanges = new SvChange[svChangeLength];
            for (int j = 0; j < svChangeLength; j++)
            {
                SvChange svChange = new SvChange();

                svChange.Time = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                offset += 4;

                svChange.Multiplier = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;

                indChart.SvChanges[j] = svChange;
            }

            int notesLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
            offset += 4;
            indChart.Notes = new NoteData[notesLength];
            for (int j = 0; j < notesLength; j++)
            {
                NoteData noteData = new NoteData();
                
                byte serializedType = bytes[offset];
                offset += 1;
                
                noteData.Time = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;
                
                noteData.Lane = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                offset += 4;
                
                switch (serializedType)
                {
                    case 1: // Typed tap note
                    {
                        noteData.Type = noteTypes[BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray())];
                        offset += 4;
                        break;
                    }
                    case 2: // Tap note with params
                    {
                        int paramCount = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                        offset += 4;
                        for (int k = 0; k < paramCount; k++)
                        {
                            int pNameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                            offset += 4;

                            StringName pName =
                                Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + pNameLength)).ToArray());
                            offset += pNameLength;

                            int pValueLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                            offset += 4;

                            Variant pValue = GD.BytesToVar(bytes.Take(new Range(offset, offset + pValueLength)).ToArray());
                            offset += pValueLength;

                            noteData.Parameters.Add(pName, pValue);
                        }
                        break;
                    }
                    case 3: // Typed tap note with params
                    {
                        noteData.Type = noteTypes[BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray())];
                        offset += 4;
                        break;
                    }
                    case 4: // Normal hold note
                    {
                        noteData.Length = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                        offset += 4;
                        break;
                    }
                    case 5: // Typed hold note
                    {
                        noteData.Length = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                        offset += 4;
                        
                        noteData.Type = noteTypes[BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray())];
                        offset += 4;
                        break;
                    }
                    case 6: // Hold note with params
                    {
                        noteData.Length = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                        offset += 4;
                        
                        int paramCount = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                        offset += 4;
                        for (int k = 0; k < paramCount; k++)
                        {
                            int pNameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                            offset += 4;

                            StringName pName =
                                Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + pNameLength)).ToArray());
                            offset += pNameLength;

                            int pValueLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                            offset += 4;

                            Variant pValue = GD.BytesToVar(bytes.Take(new Range(offset, offset + pValueLength)).ToArray());
                            offset += pValueLength;

                            noteData.Parameters.Add(pName, pValue);
                        }
                        break;
                    }
                    case 7: // Typed hold note with parameters
                    {
                        noteData.Length = BitConverter.ToSingle(bytes.Take(new Range(offset, offset + 8)).ToArray());
                        offset += 4;
                        
                        noteData.Type = noteTypes[BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray())];
                        offset += 4;
                        
                        int paramCount = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                        offset += 4;
                        for (int k = 0; k < paramCount; k++)
                        {
                            int pNameLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                            offset += 4;

                            StringName pName =
                                Encoding.UTF8.GetString(bytes.Take(new Range(offset, offset + pNameLength)).ToArray());
                            offset += pNameLength;

                            int pValueLength = BitConverter.ToInt32(bytes.Take(new Range(offset, offset + 4)).ToArray());
                            offset += 4;

                            Variant pValue = GD.BytesToVar(bytes.Take(new Range(offset, offset + pValueLength)).ToArray());
                            offset += pValueLength;

                            noteData.Parameters.Add(pName, pValue);
                        }
                        break;
                    }
                }
                
                indChart.Notes[j] = noteData;
            }

            chart.Charts[i] = indChart;
        }
    }
}