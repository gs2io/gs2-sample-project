using System;
using System.Collections;
using Google.Protobuf;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Realtime
{
    public class PlayerDescriptor : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField PlayerName;
        
        [SerializeField]
        private Button stone;
        [SerializeField]
        private Button scissors;
        [SerializeField]
        private Button paper;
        
        [SerializeField]
        private Button resetButton;
        
        public RelayRealtimeSession Session;

        public RPSState state;
        public RPSType handType;

        public void OnTapHand(int type)
        {
            switch (type)
            {
                case (int)RPSType.Stone :
                    scissors.interactable = false;
                    paper.interactable = false;
                    break;
                case (int)RPSType.Scissors :
                    stone.interactable = false;
                    paper.interactable = false;
                    break;
                case (int)RPSType.Paper :
                    stone.interactable = false;
                    scissors.interactable = false;
                    break;
            }

            handType = (RPSType)type;
            state = RPSState.Decide;
        }
        
        public void OnTapReset()
        {
            stone.interactable = true;
            scissors.interactable = true;
            paper.interactable = true;

            state = RPSState.Select;
        }

        public IEnumerator SendStatus()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.3f);
                
                ByteString binary = null;
                try
                {
                    binary = ByteString.CopyFrom(Serialize());
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    continue;
                }

                if (Session != null)
                {
                    bool lockWasTaken = false;
                    try
                    {
                        System.Threading.Monitor.TryEnter(this, ref lockWasTaken);

                        if (lockWasTaken)
                        {
                            yield return Session.UpdateProfile(
                                r => { },
                                binary
                            );
                        }
                    }
                    finally
                    {
                        if (lockWasTaken) System.Threading.Monitor.Exit(this);
                    }
                }
            }
        }

        public byte[] Serialize()
        {
            var s = BitConverter.GetBytes((int)state);
            var t = BitConverter.GetBytes((int)handType);
            var n = System.Text.Encoding.UTF8.GetBytes(PlayerName.text);

            int pos = 0;
            byte[] buffer = new byte[s.Length + t.Length + n.Length];
            Buffer.BlockCopy(s, 0, buffer, pos, s.Length);
            pos += s.Length;
            Buffer.BlockCopy(t, 0, buffer, pos, t.Length);
            pos += t.Length;
            Buffer.BlockCopy(n, 0, buffer, pos, n.Length);
            pos += n.Length;

            return buffer;
        }
    }
}