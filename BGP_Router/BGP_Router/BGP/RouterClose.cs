using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BGP_Router.Masiina;

namespace BGP_Router.BGP
{
    public class RouterClose
    {
        private Dictionary<int, Socket> ListenerSocket_DictionaryCopy = new Dictionary<int, Socket>();
        private Dictionary<int, Socket> SpeakerSocket_DictionaryCopy = new Dictionary<int, Socket>();
        UpdateMessage UpdateHandler = new UpdateMessage();
        ushort value;
        string stringValue;
        Socket socket;
        ushort tempAS;

        public void CloseSpeakerlistener(string ipAddress, int rnumber, int AS)
        {
            closeSpeaker(ipAddress);
            closelistener(ipAddress);
            Variables.removedRouteN++;
            Console.WriteLine("Key: " + Variables.removedRouteN);
            withadrawlRoutes(ipAddress, rnumber, Variables.removedRouteN, AS);
            update();
            sendNotificationMsg(Variables.removedRouteN, AS);
        }

        public void closeSpeaker(string ipAddress)
        {

            foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
            {
                try
                {
                    if (ipAddress == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()))
                    {
                        Console.WriteLine("Shutdown Connection with Speaker with IP: " + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()));
                        SpeakerSocket_DictionaryCopy.Add(speaker.Key, speaker.Value);                   
                        Variables.SpeakerAS.TryRemove("" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()), out value);
                        Variables.ConnectionAndSpeaker.TryRemove(speaker.Key, out stringValue);
                        Variables.ConnectionAndListener.TryRemove(speaker.Key, out stringValue);
                        Variables.SpeakerConnectionAndAS.TryRemove((ushort)speaker.Key, out value);
                        Variables.ListenerConnectionAndAS.TryRemove((ushort)speaker.Key, out value);
                        tempAS = value;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            foreach (KeyValuePair<int, Socket> speakercopy in SpeakerSocket_DictionaryCopy)
            {
                Variables.SpeakerSocketDictionary.TryRemove(speakercopy.Key, out socket);

            }

        }

        public void closelistener(string ipAddress)
        {

            foreach (KeyValuePair<int, Socket> listener in Variables.ListenerSocketDictionary)
            {
                try
                {
                    if (ipAddress == "" + IPAddress.Parse(((IPEndPoint)listener.Value.LocalEndPoint).Address.ToString()))
                    {
                        Console.WriteLine("Shutdown Connection with listener with IP: " + IPAddress.Parse(((IPEndPoint)listener.Value.LocalEndPoint).Address.ToString()));
                        ListenerSocket_DictionaryCopy.Add(listener.Key, listener.Value);
                        Variables.ListenerAS.TryRemove("" + IPAddress.Parse(((IPEndPoint)listener.Value.LocalEndPoint).Address.ToString()), out value);
                        Variables.ConnectionAndListener.TryRemove(listener.Key, out stringValue);
                        Variables.ConnectionAndSpeaker.TryRemove(listener.Key, out stringValue);
                        Variables.ListenerConnectionAndAS.TryRemove((ushort)listener.Key, out value);
                        Variables.SpeakerConnectionAndAS.TryRemove((ushort)listener.Key, out value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            foreach (KeyValuePair<int, Socket> listenercopy in ListenerSocket_DictionaryCopy)
            {
                Variables.ListenerSocketDictionary.TryRemove(listenercopy.Key, out socket);
            }

        }
        public void withadrawlRoutes(string ipPrefix, int rnumber, int removedRouteN, int AS)
        {
            Tuple<string, int> withdrawl_Routes = new Tuple<string, int>(ipPrefix, ipPrefix.Length);
            Console.WriteLine("Trying to remove router number: " + rnumber + ", from AS: " + AS);
            Variables.WithdrawnRoutes.Add(removedRouteN, withdrawl_Routes);
        }
        public void sendNotificationMsg(int removedRouteN, int AS)
        {
                UpdateHandler.sendNotifyMsg(removedRouteN, AS, "Router connection is removed");
        }
        public void update()
        {
            Variables.Data = Routing_table.GetTable();
            Console.WriteLine("Local Policy For AS1, AS2, and AS3 is UPDATED");
            UpdateHandler.adj_RIB_Out();
            UpdateHandler.pathAttribute();
            UpdateHandler.networkLayerReachibility();
            UpdateHandler.pathSegment();
        }
    }

}
