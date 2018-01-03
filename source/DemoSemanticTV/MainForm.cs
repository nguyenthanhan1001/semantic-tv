using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DemoSemanticTV
{
    public partial class MainForm : Form
    {
        private class Face
        {
            public Rectangle coor;
            public int ID;
        }

        Dictionary<string, List<Face>> _dict = new Dictionary<string, List<Face>>();
        string _videoFile, _metaFile, _metaUID;
        string _parentFd;
        Size _frameSize;
        int _top, _left;
        List<string> _uids = new List<string>();
        private bool _isUpdated;
        const int PAD = 25;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*";
            var res = openFileDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                _videoFile = openFileDlg.FileName;
                _parentFd = Path.GetDirectoryName(_videoFile);
                _metaFile = _videoFile.Replace(".mp4", "-meta.txt");
                _metaUID = _videoFile.Replace(".mp4", "-uid.txt");
                loadMetaData(_metaFile);
                loadUID(_metaUID);
                axWindowsMediaPlayer1.URL = _videoFile;
            }
        }

        private void loadUID(string metaUID)
        {
            foreach (string line in File.ReadLines(metaUID))
            {
                _uids.Add(line.Replace("\n", ""));
            }
        }

        private void UpdateCoordinate()
        {
            _isUpdated = true;
            _top = (axWindowsMediaPlayer1.Size.Height - axWindowsMediaPlayer1.currentMedia.imageSourceHeight) / 2;
            _left = (axWindowsMediaPlayer1.Size.Width - axWindowsMediaPlayer1.currentMedia.imageSourceWidth) / 2;
            _frameSize.Width = axWindowsMediaPlayer1.currentMedia.imageSourceWidth;
            _frameSize.Height = axWindowsMediaPlayer1.currentMedia.imageSourceHeight;
        }

        private void loadMetaData(string metaFile)
        {
            foreach (string line in File.ReadLines(metaFile))
            {
                string[] tmp = line.Split('\t');
                string[] coor = tmp[1].Split(' ');
                Face f = new Face();
                f.ID = int.Parse(tmp[2]);
                f.coor = new Rectangle(int.Parse(coor[0]), int.Parse(coor[1]), int.Parse(coor[2]), int.Parse(coor[3]));

                if (!_dict.ContainsKey(tmp[0]))
                {
                    _dict[tmp[0]] = new List<Face>();
                }
                _dict[tmp[0]].Add(f);
            }
        }

        private void forwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition += 10;
        }

        private void moveBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition -= 10;
        }

        private void axWindowsMediaPlayer1_ClickEvent(object sender, AxWMPLib._WMPOCXEvents_ClickEvent e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
                if (!_isUpdated)
                {
                    UpdateCoordinate();
                }
                processOnVideoClick(e.fX - _left, e.fY - _top);
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void processOnVideoClick(int fX, int fY)
        {
            string pos = axWindowsMediaPlayer1.Ctlcontrols.currentPositionString;
            if (_dict.ContainsKey(pos))
            {
                foreach (Face face in _dict[pos])
                {
                    if (fX >= face.coor.Left - PAD && fX <= face.coor.Left + face.coor.Width + PAD
                    && fY >= face.coor.Top - PAD && fY <= face.coor.Top + face.coor.Height + PAD)
                    {
                        MessageBox.Show(_uids[face.ID]);
                        break;
                    }
                }
            }
        }
    }
}
