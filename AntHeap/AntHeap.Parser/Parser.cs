using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AntHeap.Parser
{
    class Parser
    {
        private readonly string _in;
        private readonly string _out;
        private readonly byte _type;
        private readonly StreamWriter[] _outputs;
        const int NTypes=256;

        public Parser(string @in, string @out, byte type)
        {
            _in = @in;
            _out = @out;
            _type = type;
            _outputs = new StreamWriter[NTypes];
        }

        StreamWriter GetStream(int tp) {
            if(_outputs[tp]==null) {
                string fn=Path.Combine(_out,"area-" + tp.ToString("D3"));
                _outputs[tp]=new StreamWriter(fn);
            }
            return _outputs[tp];
        }

        string m_NextCell,m_NextCellType,m_NextAntCell;
        Guid m_NextAntGUID;
        TextReader m_AntsToCellsStream,m_CellsStream;
        HashSet<Guid> _ants;
        public void Parse()
        {
            Console.WriteLine("Parsing ants of type {0} from {1} to {2}", _type, _in, _out);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            _ants = new HashSet<Guid>(ReadAnts());
            m_AntsToCellsStream=ReadInput("antsToCells");
            m_CellsStream=ReadInput("Cells");

            ReadAntToCell();
            ReadCell();

            while(m_NextCell!=null && m_NextAntCell!=null) {
                int k=m_NextAntCell.CompareTo(m_NextCell);
                if(k>0)
                    ReadCell();
                else {
                    if(k==0)
                        SaveAnt();
                    else
                        throw new InvalidOperationException("Can't find cell");
                    ReadAntToCell();
                }
            }

            foreach(var v in _outputs) if(v!=null)
                    v.Close();
            sw.Stop();
            Console.WriteLine("Parsing complete in " + sw.Elapsed.ToString("mm\\:ss\\.fff"));
        }

        private TextReader ReadInput(string file)
        {
            return new StreamReader(new FileStream(Path.Combine(_in, file), FileMode.Open, FileAccess.Read));
        }

        private IEnumerable<Guid> ReadAnts()
        {
            using (var r = ReadInput("ants"))
            {
                var buffer = new char[38];
                while (r.Read(buffer, 0, 38) == 38)
                {
                    if (buffer[32] != '\t' || buffer[36] != '\r' || buffer[37] != '\n')
                        throw new InvalidOperationException("Wrong file format");
                    if (byte.Parse(new string(buffer, 33, 3)) != _type)
                        continue;
                    yield return Guid.ParseExact(new string(buffer, 0, 32), "N");
                }
            }
        }


        const int LCellRecord=38;
        char[] m_cellBuffer=new char[LCellRecord];
        void ReadCell() {
            if(m_CellsStream.Read(m_cellBuffer,0,LCellRecord)<LCellRecord) {
                m_NextCell=m_NextCellType=null;
            } else {
                if(m_cellBuffer[32] != '\t' || m_cellBuffer[36] != '\r' || m_cellBuffer[37] != '\n')
                    throw new InvalidOperationException("Wrong file format");
                m_NextCell=new string(m_cellBuffer,0,32);
                m_NextCellType=new string(m_cellBuffer,33,3);
            }
        }

        const int LAntToCellRecord=67;
        char[] m_antBuffer=new char[LAntToCellRecord];
        void ReadAntToCell() {
            for(;;) {
                if(m_AntsToCellsStream.Read(m_antBuffer,0,LAntToCellRecord)<LAntToCellRecord) {
                    m_NextAntCell=null;
                    return;
                } else {
                    if(m_antBuffer[32] != '\t' || m_antBuffer[65] != '\r' || m_antBuffer[66] != '\n')
                        throw new InvalidOperationException("Wrong file format");
                    m_NextAntGUID=Guid.ParseExact(new string(m_antBuffer,0,32),"N");
                    if(_ants.Contains(m_NextAntGUID)) {
                        m_NextAntCell=new string(m_antBuffer,33,32);
                        return;
                    }
                }
            }
        }

        private void SaveAnt() {
            int type=int.Parse(m_NextCellType);
            var writer =GetStream(type);
            writer.Write(m_NextAntGUID.ToString("N"));
            writer.Write('\t');
            writer.WriteLine(m_NextCell);
        }
    }
}
