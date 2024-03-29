using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AlturaNFT  

{ 
    
    using Internal;
    /// <summary>
    /// Details of particular NFT
    /// </summary>
    [AddComponentMenu(AlturaConstants.BaseComponentMenu+AlturaConstants.FeatureName_User_Details)]
    [ExecuteAlways]
    [HelpURL(AlturaConstants.Docs_UserDetails)]
    public class User_Details : MonoBehaviour
    {
        /// <summary>
        /// Currently Supported chains for this endpoint.
        /// </summary>
        public enum Chains
        {
            ethereum,
            binance,
            bsctest,
            rinkeby,
        }

        #region Parameter Defines

            [SerializeField]
            private Chains chain = Chains.bsctest;
            
            [SerializeField]
            [DrawIf("chain", Chains.binance , DrawIfAttribute.DisablingType.DontDrawInverse)]
            private string _address = "Input Address of the Altura User";
            
            

            private string RequestUriInit = "https://api.alturanft.com/api/v2/user/";
            private string WEB_URL;
            private string _apiKey;
            private bool destroyAtEnd = false;


            private UnityAction<string> OnErrorAction;
            private UnityAction<User_model> OnCompleteAction;
            
            [Space(20)]
            //[Header("Called After Successful API call")]
            public UnityEvent afterSuccess;
            //[Header("Called After Error API call")]
            public UnityEvent afterError;

            [Header("Run Component when this Game Object is Set Active")]
            [SerializeField] private bool onEnable = false;
            public bool debugErrorLog = true;
            public bool debugLogRawApiResponse = false;
            
            [Header("Gets filled with data and can be referenced:")]
            public User_model users;

        #endregion


        private void Awake()
        {
            AlturaUser.Initialise();
            _apiKey = AlturaUser.GetUserApiKey();
            
        }

        private void OnEnable()
        {
            if (onEnable & Application.isPlaying)
            {
                AlturaUser.SetFromOnEnable();
                Run();
            }
        }

        #region SetParams and Chain Functions

        /// <summary>
        /// Initialize creates a gameobject and assings this script as a component. This must be called if you are not refrencing the script any other way and it doesn't already exists in the scene.
        /// </summary>
        /// <param name="destroyAtEnd"> Optional bool parameter can set to false to avoid Spawned GameObject being destroyed after the Api process is complete. </param>
        public static User_Details Initialize(bool destroyAtEnd = true)
            {
                var _this = new GameObject(AlturaConstants.FeatureName_User_Details).AddComponent<User_Details>();
                _this.destroyAtEnd = destroyAtEnd;
                _this.onEnable = false;
                _this.debugErrorLog = false;
                return _this;
            }

        /// <summary>
        /// Set Parameters to retrieve User From.  ≧◔◡◔≦ .
        /// </summary>
        /// <param name="_address"> as string - EVM</param>
        public User_Details SetParameters(string _addresss = null)
            {
                if(_addresss!=null)
                    this._address = _addresss;
     

                return this;
            }
            
            /// <summary>
            /// Blockchain from which to query NFTs.
            /// </summary>
            /// <param name="chain"> Choose from available 'Chains' enum</param>
            public User_Details SetChain(Chains chain)
            {
                this.chain = chain;
                return this;
            }

            /// <summary>
            /// Action on successful API Fetch. (*^∇^)ヾ(￣▽￣*)
            /// </summary>
            /// <param name="NFTs_OwnedByAnAccount_model.Root"> Use: .OnComplete(NFTs=> NFTsOfUser = NFTs) , where NFTsOfUser = NFTs_OwnedByAnAccount_model.Root;</param>
            /// <returns> NFTs_OwnedByAnAccount_model.Root </returns>
            public User_Details OnComplete(UnityAction<User_model> action)
            {
                this.OnCompleteAction = action;
                return this;
            }
            
            /// <summary>
            /// Action on Error (⊙.◎)
            /// </summary>
            /// <param name="UnityAction action"> string.</param>
            /// <returns> Information on Error as string text.</returns>
            public User_Details OnError(UnityAction<string> action)
            {
                this.OnErrorAction = action;
                return this;
            }
            
        #endregion

        
        #region Run - API
            /// <summary>
            /// Runs the Api call and fills the corresponding model in the component on success.
            /// </summary>
            public User_model Run()
            {
                WEB_URL = BuildUrl();
                StopAllCoroutines();
                StartCoroutine(CallAPIProcess());
                return users;
            }

            string BuildUrl()
            {
                if (chain == Chains.bsctest)
                {
                    WEB_URL = RequestUriInit + _address;
                    if(debugErrorLog)
                        Debug.Log("Querying Details of User: " + _address + " on " + chain);
                }
                else
                {
                    WEB_URL = RequestUriInit + _address;
                    if(debugErrorLog)
                        Debug.Log("Querying Details of User: " + _address +  " on " + chain);
                } 
                return WEB_URL;
            }
            
            IEnumerator CallAPIProcess()
            {
                //Make request
                UnityWebRequest request = UnityWebRequest.Get(WEB_URL);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("source", AlturaUser.GetSource());
                request.SetRequestHeader("Authorization", _apiKey);
                

                {
                    yield return request.SendWebRequest();
                    string jsonResult = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                    
                    if(debugLogRawApiResponse)
                        Debug.Log(jsonResult);

                    if (request.error != null)
                    {
                        if(OnErrorAction!=null)
                            OnErrorAction($"Null data. Response code: {request.responseCode}. Result {jsonResult}");
                        if(debugErrorLog)
                            Debug.Log($"(⊙.◎) Null data. Response code: {request.responseCode}. Result {jsonResult}");
                        if(afterError!=null)
                            afterError.Invoke();
                        users = null;
                        //yield break;
                    }
                    else
                    {
                        
                        users = JsonConvert.DeserializeObject<User_model>(
                            jsonResult,
                            new JsonSerializerSettings
                            {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                            });
                        
                        if(OnCompleteAction!=null)
                            OnCompleteAction.Invoke(users);
                        
                        if(afterSuccess!=null)
                            afterSuccess.Invoke();
                        
                            Debug.Log($" ´ ▽ ` )ﾉ Success , view User under User model" );
                    }
                }
                request.Dispose();
                if(destroyAtEnd)
                    Destroy (this.gameObject);
            }
            
        #endregion
    }

}
