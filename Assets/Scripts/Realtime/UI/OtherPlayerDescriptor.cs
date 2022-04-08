using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gs2.Sample.Realtime
{
    public class OtherPlayerDescriptor : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerName;

        [SerializeField]
        private Image stone;
        [SerializeField]
        private Image scissors;
        [SerializeField]
        private Image paper;
        
        public RPSState state;
        public RPSType handType;

        private RPSState _membersState;
        
        private void Start()
        {
            stone.gameObject.SetActive(false);
            scissors.gameObject.SetActive(false);
            paper.gameObject.SetActive(false);
        }

        void Update()
        {
            RPSType _handType;
            if (_membersState == RPSState.Select)
                _handType = (RPSType) Random.Range(0, 3);
            else
                _handType = handType;
            
            switch (_handType)
            {
                case RPSType.Stone :
                    stone.gameObject.SetActive(true);
                    scissors.gameObject.SetActive(false);
                    paper.gameObject.SetActive(false);
                    break;
                case RPSType.Scissors :
                    stone.gameObject.SetActive(false);
                    scissors.gameObject.SetActive(true);
                    paper.gameObject.SetActive(false);
                    break;
                case RPSType.Paper :
                    stone.gameObject.SetActive(false);
                    scissors.gameObject.SetActive(false);
                    paper.gameObject.SetActive(true);
                    break;
            }
        }

        public void ProfileDeserialize(byte[] data)
        {
            var pos = 0;

            if (playerName != null)
            {
                playerName.SetText(Encoding.UTF8.GetString(data, pos, data.Length - pos));
            }
        }

        public void StateDeserialize(byte[] data)
        {
            var pos = 0;

            state = (RPSState)BitConverter.ToInt32(data, pos);
            handType = (RPSType)BitConverter.ToInt32(data, pos + 4);
        }
        
        public void SetMembersState(RPSState state)
        {
            _membersState = state;
        }
    }
}