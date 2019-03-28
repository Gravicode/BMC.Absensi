using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.IO;

namespace BMC.AbsenDevice
{
    public partial class Program
    {
        const string UrlWeb = "http://bmcsecurityweb.azurewebsites.net/Scan?IDS=";
        public bool IsConnected { get; set; }
        static Queue DataRFID = new Queue();

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            rfidReader.IdReceived += rfidReader_IdReceived;
            characterDisplay.BacklightEnabled = true;
           
            JoinNetwork();
        }
        void ProcessQue()
        {
            while (true)
            {
                while (DataRFID.Count > 0)
                {
                    var que = DataRFID.Dequeue();
                    if (que != null)
                    {
                        var data = que as string;
                        PushData(data);
                    }
                    else
                    {
                        break;
                    }
                }
                Thread.Sleep(2000);
            }
        }
        void JoinNetwork()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;


            wifiRS21.NetworkInterface.Open();
            wifiRS21.NetworkInterface.EnableDhcp();
            wifiRS21.NetworkInterface.EnableDynamicDns();
            wifiRS21.NetworkInterface.Join("BMC123", "bmc123qweasd");
            Print("try to connect..");
            while (wifiRS21.NetworkInterface.IPAddress == "0.0.0.0")
            {
                Debug.Print("Waiting for DHCP");
                Thread.Sleep(250);
            }
            Print("IP:" + wifiRS21.NetworkInterface.IPAddress);
            //The network is now ready to use.
            //start process
            Thread th1 = new Thread(new ThreadStart(ProcessQue));
            th1.Start();
        }
        void Print(string Message)
        {
            characterDisplay.Clear();
            characterDisplay.Print(Message);
            Debug.Print(Message);
        }

        void rfidReader_IdReceived(RFIDReader sender, string e)
        {
            DataRFID.Enqueue(e);
            Print("scan:" + e);
            PlayDing();
        }

        private static void NetworkChange_NetworkAddressChanged(object sender, Microsoft.SPOT.EventArgs e)
        {
            Debug.Print("Network address changed");
        }
        void PlayDing()
        {
            //Tunes.MusicNote note = new Tunes.MusicNote(Tunes.Tone.C4, 400);

            //tunes.AddNote(note);

            //// up
            //PlayNote(Tunes.Tone.C4);
            //PlayNote(Tunes.Tone.D4);
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.F4);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.A4);
            //PlayNote(Tunes.Tone.B4);
            //PlayNote(Tunes.Tone.C5);

            //// back down
            //PlayNote(Tunes.Tone.B4);
            //PlayNote(Tunes.Tone.A4);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.F4);
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.D4);
            //PlayNote(Tunes.Tone.C4);

            //// arpeggio
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.C5);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.C4);

            //tunes.Play();

            //Thread.Sleep(100);

            Tunes.MusicNote[] notes = new Tunes.MusicNote[] { 
            new Tunes.MusicNote(Tunes.Tone.E4, 200),
            new Tunes.MusicNote(Tunes.Tone.G4, 200),
            new Tunes.MusicNote(Tunes.Tone.C5, 200),
            new Tunes.MusicNote(Tunes.Tone.G4, 200),
            new Tunes.MusicNote(Tunes.Tone.E4, 200),
            new Tunes.MusicNote(Tunes.Tone.C4, 200)
            };


            tunes.Play(notes);
        }
        void PlayNote(Tunes.Tone tone)
        {
            Tunes.MusicNote note = new Tunes.MusicNote(tone, 200);

            tunes.AddNote(note);
        }
        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Debug.Print("Network availability: " + e.IsAvailable.ToString());
        }
        void PushData(string STRID)
        {
            try
            {
                byte[] result = new byte[65536];
                int read = 0;

                using (var req = HttpWebRequest.Create(UrlWeb + STRID) as HttpWebRequest)
                {
                    using (var res = req.GetResponse() as HttpWebResponse)
                    {
                        using (var stream = res.GetResponseStream())
                        {
                            do
                            {
                                read = stream.Read(result, 0, result.Length);

                                Thread.Sleep(20);
                            } while (read != 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }
    }
    public class Absen
    {
        public string IDS { get; set; }
    }
}
