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
        public void CloseSpeakerlistener(string ipAddress, int adj, int AS)
        {
            closeSpeaker(ipAddress);
            closelistener(ipAddress);
            withadrawlRoutes(ipAddress, value);
            update();
            sendNotificationMsg(adj, AS);

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
                        // Release the socket.                       
                        Variables.SpeakerAS.TryRemove("" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()), out value);
                        Variables.ConnectionAndSpeaker.TryRemove(speaker.Key, out stringValue);
                        Variables.ConnectionAndListener.TryRemove(speaker.Key, out stringValue);
                        Variables.SpeakerConnectionAndAS.TryRemove((ushort)speaker.Key, out value);
                        Variables.ListenerConnectionAndAS.TryRemove((ushort)speaker.Key, out value);
                        tempAS = value;

                        //Variables.SpeakerSocket_Dictionary.Remove(speaker.Key);
                        //speaker.Value.Dispose();


                        /**
                        speaker.Value.BeginDisconnect(true, DisconnectSpeakerCallback, speaker.Value);

                        // Wait for the disconnect to complete.
                        disconnectSpeakerDone.WaitOne();
                        if (speaker.Value.Connected)
                            Console.WriteLine("We're still connected");
                        else
                            Console.WriteLine("We're disconnected");
                        speaker.Value.Shutdown(SocketShutdown.Both);
                        speaker.Value.Close();
                        **/
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
                        // Release the socket.

                        //Variables.ListenerSocketDictionary.Remove(listener.Key);
                        ListenerSocket_DictionaryCopy.Add(listener.Key, listener.Value);
                        Variables.ListenerAS.TryRemove("" + IPAddress.Parse(((IPEndPoint)listener.Value.LocalEndPoint).Address.ToString()), out value);
                        Variables.ConnectionAndListener.TryRemove(listener.Key, out stringValue);
                        Variables.ConnectionAndSpeaker.TryRemove(listener.Key, out stringValue);
                        Variables.ListenerConnectionAndAS.TryRemove((ushort)listener.Key, out value);
                        Variables.SpeakerConnectionAndAS.TryRemove((ushort)listener.Key, out value);
                        //UpdateHandler.withadrawlRoutes(ipAddress);
                        //listener.Value.Dispose();
                        /**
                        listener.Value.BeginDisconnect(true, DisconnectlistenerCallback, listener.Value);

                        // Wait for the disconnect to complete.
                        disconnectlistenerDone.WaitOne();
                        if (listener.Value.Connected)
                            Console.WriteLine("We're still connected");
                        else
                            Console.WriteLine("We're disconnected");
                        listener.Value.Shutdown(SocketShutdown.Both);
                        listener.Value.Close();
        **/
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
        public void withadrawlRoutes(string ipPrefix, int AS)
        {
            //Variables.withdrawnRoutes.Clear();
            Tuple<string, int> withdrawl_Routes = new Tuple<string, int>(ipPrefix, ipPrefix.Length);
            //Variables.withdrawl_IP_Address = ipPrefix;
            Variables.WithdrawnRoutes.Add(AS, withdrawl_Routes);
        }
        public void sendNotificationMsg(int adj, int AS)
        {
            if (adj == 1)
            {
                UpdateHandler.sendNotifyMsg(adj, AS, "Router conection is Ceased");
            }
            if (adj == 3)
            {
                UpdateHandler.sendNotifyMsg(adj, AS, "Router conection is Ceased");
            }
            if (adj == 4)
            {
                UpdateHandler.sendNotifyMsg(adj, AS, "Router conection is Ceased");
            }

        }
        public void update()
        {
            Variables.Data = Routing_table.GetTable();
            Console.WriteLine("Local Policy For AS1, AS2 and AS3 is UPDATED");
            UpdateHandler.adj_RIB_Out();
            //createUpdate.withadrawlRoutes("");
            UpdateHandler.pathAttribute();
            UpdateHandler.networkLayerReachibility();
            UpdateHandler.pathSegment();
        }
        /**

        private static void DisconnectSpeakerCallback(IAsyncResult ar)
        {
            // Complete the disconnect request.
            Socket client = (Socket)ar.AsyncState;
            client.EndDisconnect(ar);
            //client.Close();
            // Signal that the disconnect is complete.
            disconnectSpeakerDone.Set();
           
        }

        private static void DisconnectlistenerCallback(IAsyncResult ar)
        {
            // Complete the disconnect request.
            Socket listener = (Socket)ar.AsyncState;
            listener.EndDisconnect(ar);
            //listener.Close();
            // Signal that the disconnect is complete.
            disconnectlistenerDone.Set();
        }

    **/
    }

}
