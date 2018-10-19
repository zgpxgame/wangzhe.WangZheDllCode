using Apollo;
using Assets.Scripts.Framework;
using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class TdirMgr : MonoSingleton<TdirMgr>
{
	public delegate void TdirManagerEvent();

	private const int RootNodeID = 0;

	private const int PrivateNodeID = 1;

	private const int TestNodeID = 2;

	private const int MaxSyncTime = 3;

	private const string LoginTimeKey = "LoginTimeKey";

	private const string LoginFailTimesKey = "LoginFailTimesKey";

	private const int FailTimesLimit1 = 3;

	private const int FailTimesLimit2 = 6;

	private const int FailTimesDuration1 = 10;

	private const int FailTimesDuration2 = 300;

	public const int RegetTime = 200;

	public TdirMgr.TdirManagerEvent SvrListLoaded;

	private static RootNodeType SNodeType;

	public static bool s_maintainBlock = true;

	private static bool mIsCheckVersion;

	private int mCurrentNodeIDFirst = 1;

	private ApolloTdir mTdir;

	private ApolloTdir mTdirISP;

	private TreeCommonData mCommonData;

	private List<TdirTreeNode> mTreeNodes;

	private int recvTimeout = 10;

	private int apolloTimeout = 10000;

	private int syncTimeOut = 12;

	public string mRootNodeAppAttr;

	private List<TdirSvrGroup> mSvrListGroup;

	private TdirSvrGroup mRecommondUrlList;

	private TdirSvrGroup mOwnRoleList;

	private TdirSvrGroup mPrivateSvrList;

	private TdirSvrGroup mTestSvrList;

	private AsyncStatu mAsyncFinished;

	private ApolloPlatform mPlatform;

	private int mLastLoginNodeID;

	private TdirUrl mLastLoginUrl;

	private TdirUrl mSelectedTdir;

	private TdirEnv mTdirEnv = TdirEnv.Release;

	public bool EnableMarkLastLogin = true;

	private int mSyncTime;

	private int mFailTimes;

	private DictionaryView<string, List<TdirTreeNode>> mOpenIDTreeNodeDic = new DictionaryView<string, List<TdirTreeNode>>();

	public float m_regetTime;

	public string[] m_iplist;

	public int[] m_portlist;

	private IAsyncResult getAddressResult;

	private ApolloAccountInfo m_info;

	public int m_connectIndex = -1;

	public bool isPreEnabled;

	public string preVersion;

	public bool isUseHttpDns = true;

	private float m_GetTdirTime;

	private TdirResult m_GetinnerResult = TdirResult.NoServiceTable;

	private static int InternalNodeID
	{
		get
		{
			return (!TdirMgr.mIsCheckVersion) ? 1 : 1;
		}
	}

	private static int QQNodeID
	{
		get
		{
			if (TdirMgr.SNodeType == RootNodeType.Normal)
			{
				return (!TdirMgr.mIsCheckVersion) ? 2 : 5;
			}
			if (TdirMgr.SNodeType == RootNodeType.TestFlight)
			{
				return 8;
			}
			if (TdirMgr.SNodeType == RootNodeType.TestSpecial)
			{
				return 11;
			}
			return (!TdirMgr.mIsCheckVersion) ? 2 : 5;
		}
	}

	private static int WeixinNodeID
	{
		get
		{
			if (TdirMgr.SNodeType == RootNodeType.Normal)
			{
				return (!TdirMgr.mIsCheckVersion) ? 3 : 6;
			}
			if (TdirMgr.SNodeType == RootNodeType.TestFlight)
			{
				return 9;
			}
			if (TdirMgr.SNodeType == RootNodeType.TestSpecial)
			{
				return 12;
			}
			return (!TdirMgr.mIsCheckVersion) ? 3 : 6;
		}
	}

	private static int GuestNodeID
	{
		get
		{
			if (TdirMgr.SNodeType == RootNodeType.Normal)
			{
				return (!TdirMgr.mIsCheckVersion) ? 4 : 7;
			}
			if (TdirMgr.SNodeType == RootNodeType.TestFlight)
			{
				return 10;
			}
			if (TdirMgr.SNodeType == RootNodeType.TestSpecial)
			{
				return 13;
			}
			return (!TdirMgr.mIsCheckVersion) ? 4 : 7;
		}
	}

	public static bool IsCheckVersion
	{
		get
		{
			return TdirMgr.mIsCheckVersion;
		}
	}

	public List<TdirSvrGroup> SvrUrlList
	{
		get
		{
			return this.mSvrListGroup;
		}
	}

	public TdirSvrGroup RecommondUrlList
	{
		get
		{
			return this.mRecommondUrlList;
		}
	}

	public TdirSvrGroup OwnRoleList
	{
		get
		{
			return this.mOwnRoleList;
		}
	}

	public AsyncStatu AsyncFinished
	{
		get
		{
			return this.mAsyncFinished;
		}
	}

	public TdirUrl LastLoginUrl
	{
		get
		{
			return this.mLastLoginUrl;
		}
	}

	public TdirUrl SelectedTdir
	{
		get
		{
			return this.mSelectedTdir;
		}
	}

	private int FailTimes
	{
		get
		{
			return this.mFailTimes;
		}
		set
		{
			PlayerPrefs.SetInt("LoginFailTimesKey", value);
			this.mFailTimes = value;
		}
	}

	protected override void Init()
	{
		this.mTdir = new ApolloTdir();
		this.mTdirISP = new ApolloTdir();
		this.ResetISPData();
		if (PlayerPrefs.HasKey("LoginFailTimesKey"))
		{
			this.FailTimes = PlayerPrefs.GetInt("LoginFailTimesKey");
		}
		this.apolloTimeout = PlayerPrefs.GetInt(TdirNodeAttrType.tdirTimeOut.ToString(), this.apolloTimeout);
		this.syncTimeOut = PlayerPrefs.GetInt(TdirNodeAttrType.tdirSyncTimeOut.ToString(), this.syncTimeOut);
		Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.TDir_QuitGame, new CUIEventManager.OnUIEventHandler(this.OnQuitGame));
		Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.TDir_TryAgain, new CUIEventManager.OnUIEventHandler(this.OnTryAgain));
		Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.TDir_ConnectLobby, new CUIEventManager.OnUIEventHandler(this.OnTDirConnectLobby));
	}

	public void Dispose()
	{
		Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.TDir_QuitGame, new CUIEventManager.OnUIEventHandler(this.OnQuitGame));
		Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.TDir_TryAgain, new CUIEventManager.OnUIEventHandler(this.OnTryAgain));
		Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.TDir_ConnectLobby, new CUIEventManager.OnUIEventHandler(this.OnTDirConnectLobby));
	}

	private void OnQuitGame(CUIEvent uiEvent)
	{
		SGameApplication.Quit();
	}

	private void OnTryAgain(CUIEvent uiEvent)
	{
		this.TdirAsync(this.m_info, null, null, true);
	}

	private void OnTDirConnectLobby(CUIEvent uiEvent)
	{
		this.ConnectLobby();
	}

	public void TdirAsync(ApolloAccountInfo info, Action successCallBack, Action failCallBack, bool reasync)
	{
		this.preVersion = null;
		this.isPreEnabled = false;
		this.isUseHttpDns = true;
		this.m_info = info;
		this.mSyncTime = 0;
		this.SetIpAndPort();
		this.m_GetTdirTime = Time.time;
		base.StartCoroutine(this.QueryTdirAsync(info, successCallBack, failCallBack, reasync));
	}

	public void TdirAsyncISP()
	{
		this.SetIpAndPort();
		base.StartCoroutine(this.QueryISP());
	}

	public void SetIpAndPort()
	{
		this.m_iplist = TdirConfig.GetTdirIPList();
		this.m_portlist = TdirConfig.GetTdirPortList();
	}

	private void InitTdir(ApolloPlatform platform)
	{
		this.mPlatform = platform;
		this.InitSvrList();
	}

	public void EnterGame(TdirUrl tdirUrl)
	{
		this.mSelectedTdir = tdirUrl;
		this.SetConfigAtClickTdir(this.mSelectedTdir);
	}

	private void InitSvrList()
	{
		this.m_connectIndex = -1;
		if (this.mPlatform == ApolloPlatform.QQ)
		{
			this.mCurrentNodeIDFirst = TdirMgr.QQNodeID;
		}
		else if (this.mPlatform == ApolloPlatform.Wechat)
		{
			this.mCurrentNodeIDFirst = TdirMgr.WeixinNodeID;
		}
		else if (this.mPlatform == ApolloPlatform.Guest)
		{
			this.mCurrentNodeIDFirst = TdirMgr.GuestNodeID;
		}
		this.ResetLastLoginUrl();
		this.ResetSelectedUrl();
		this.mOwnRoleList = default(TdirSvrGroup);
		this.mOwnRoleList.name = Singleton<CTextManager>.instance.GetText("Tdir_My_Svr");
		this.mOwnRoleList.tdirUrls = new List<TdirUrl>();
		this.mRecommondUrlList = default(TdirSvrGroup);
		this.mRecommondUrlList.name = Singleton<CTextManager>.instance.GetText("Tdir_Rcmd_Svr");
		this.mRecommondUrlList.tdirUrls = new List<TdirUrl>();
		this.mPrivateSvrList = default(TdirSvrGroup);
		this.mPrivateSvrList.name = "PrivateSvrList";
		this.mPrivateSvrList.nodeID = this.GetNodeIDByPos(this.mCurrentNodeIDFirst, 1, 0, 0);
		this.mPrivateSvrList.tdirUrls = new List<TdirUrl>();
		this.mTestSvrList = default(TdirSvrGroup);
		this.mTestSvrList.name = "TestSvrList";
		this.mTestSvrList.nodeID = this.GetNodeIDByPos(this.mCurrentNodeIDFirst, 2, 0, 0);
		this.mTestSvrList.tdirUrls = new List<TdirUrl>();
		this.mRootNodeAppAttr = null;
		this.mSvrListGroup = new List<TdirSvrGroup>();
		this.mSvrListGroup.Add(this.mOwnRoleList);
		this.mSvrListGroup.Add(this.mRecommondUrlList);
		if (this.mTdirEnv == TdirEnv.Test)
		{
			this.mSvrListGroup.Add(this.mPrivateSvrList);
			this.mSvrListGroup.Add(this.mTestSvrList);
		}
		this.ParseNodeInfo();
		this.mOwnRoleList.tdirUrls.Sort(new Comparison<TdirUrl>(this.SortTdirUrl));
		this.CheckHttpDns();
		if (this.mLastLoginUrl.nodeID > 0 && this.LastLoginUrl.name != null && this.LastLoginUrl.name.Length != 0)
		{
			this.mSelectedTdir = this.mLastLoginUrl;
		}
		else if (this.mRecommondUrlList.tdirUrls.Count > 0)
		{
			this.mSelectedTdir = this.mRecommondUrlList.tdirUrls[this.mRecommondUrlList.tdirUrls.Count - 1];
		}
		else if (this.mSvrListGroup.Count > 0 && this.mSvrListGroup[this.mSvrListGroup.Count - 1].tdirUrls.Count > 0)
		{
			int count = this.mSvrListGroup[this.mSvrListGroup.Count - 1].tdirUrls.Count;
			this.mSelectedTdir = this.mSvrListGroup[this.mSvrListGroup.Count - 1].tdirUrls[count - 1];
		}
		if (this.SvrListLoaded != null)
		{
			this.SvrListLoaded();
		}
	}

	public TdirUrl GetDefaultTdirUrl()
	{
		if (this.CheckTdirUrlValid(this.mLastLoginUrl))
		{
			return this.mLastLoginUrl;
		}
		if (this.RecommondUrlList.tdirUrls != null)
		{
			for (int i = 0; i < this.RecommondUrlList.tdirUrls.Count; i++)
			{
				if (this.CheckTdirUrlValid(this.RecommondUrlList.tdirUrls[i]))
				{
					return this.RecommondUrlList.tdirUrls[i];
				}
			}
		}
		if (this.mSvrListGroup != null)
		{
			for (int j = 0; j < this.mSvrListGroup.Count; j++)
			{
				if (this.mSvrListGroup[j].tdirUrls != null)
				{
					for (int k = 0; k < this.mSvrListGroup[j].tdirUrls.Count; k++)
					{
						if (this.CheckTdirUrlValid(this.mSvrListGroup[j].tdirUrls[k]))
						{
							return this.mSvrListGroup[j].tdirUrls[k];
						}
					}
				}
			}
		}
		return default(TdirUrl);
	}

	public bool CheckTdirUrlValid(TdirUrl url)
	{
		return url.nodeID != 0 && url.addrs != null;
	}

	private int SortTdirUrl(TdirUrl url1, TdirUrl url2)
	{
		int nodeID = url1.nodeID;
		int nodeID2 = url2.nodeID;
		if (nodeID > nodeID2)
		{
			return 1;
		}
		if (nodeID == nodeID2)
		{
			return 0;
		}
		return -1;
	}

	private bool CheckTreeNodeValid(TdirTreeNode node)
	{
		if (node.staticInfo.cltAttr1 == 0)
		{
			return false;
		}
		object obj = new object();
		IPAddress iPAddress;
		if (IPAddress.TryParse(GameFramework.AppVersion, out iPAddress))
		{
			int num = BitConverter.ToInt32(iPAddress.GetAddressBytes(), 0);
			return (!this.GetTreeNodeAttr(node, TdirNodeAttrType.versionDown, ref obj) || (int)obj <= num) && (!this.GetTreeNodeAttr(node, TdirNodeAttrType.versionUp, ref obj) || (int)obj >= num);
		}
		return true;
	}

	private bool CheckEnterTdirUrl(TdirUrl tdirUrl, bool tips = false)
	{
		if (tdirUrl.nodeID == 0 || tdirUrl.statu == TdirSvrStatu.UNAVAILABLE || tdirUrl.addrs == null)
		{
			if (tips)
			{
			}
			return false;
		}
		return true;
	}

	private bool GetLastLoginNode(int nodeID, ref TdirUrl url)
	{
		if (this.mSvrListGroup == null || nodeID == 0 || (this.mPlatform == ApolloPlatform.QQ && this.GetIPPosByNodeID(nodeID, 0) != TdirMgr.QQNodeID) || (this.mPlatform == ApolloPlatform.Wechat && this.GetIPPosByNodeID(nodeID, 0) != TdirMgr.WeixinNodeID) || (this.mPlatform == ApolloPlatform.Guest && this.GetIPPosByNodeID(nodeID, 0) != TdirMgr.GuestNodeID))
		{
			url.nodeID = 0;
			return false;
		}
		for (int i = 0; i < this.mSvrListGroup.Count; i++)
		{
			List<TdirUrl> tdirUrls = this.mSvrListGroup[i].tdirUrls;
			for (int j = 0; j < tdirUrls.Count; j++)
			{
				if (tdirUrls[j].nodeID == nodeID)
				{
					return this.CreateSvrUrl(tdirUrls[j], ref url);
				}
			}
		}
		return false;
	}
	
    private IEnumerator QueryTdirAsync(ApolloAccountInfo info, Action successCallBack, Action failCallBack, bool reasync)
    {
		float time = 0;
		if (mAsyncFinished != AsyncStatu.IsAsyncing)
		{
			mAsyncFinished = AsyncStatu.AsyncFail;
			if (info == null)
			{
				if (failCallBack != null)
				{
					failCallBack();
				}
			}
			else
			{
				if (!reasync && mOpenIDTreeNodeDic.ContainsKey(info.OpenId + info.Platform))
				{
					mTreeNodes = mOpenIDTreeNodeDic[info.OpenId + info.Platform];
					mAsyncFinished = AsyncStatu.AsyncSuccess;
					goto Label_03BF;
				}
					
				var queryResult = mTdir.Query(TdirConfig.GetTdirAppId(), m_iplist, m_portlist, string.Empty, 0, info.OpenId, false);
				mTdir.SetSvrTimeout(apolloTimeout);
				if (queryResult != TdirResult.TdirNoError)
				{
					if (failCallBack != null)
					{
						failCallBack();
					}
					Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("ChooseSvr_WeakNetwork"), enUIEventID.TDir_TryAgain, enUIEventID.TDir_QuitGame, Singleton<CTextManager>.GetInstance().GetText("ChooseSvr_Retry"), Singleton<CTextManager>.GetInstance().GetText("ChooseSvr_Exit"), false);
				}
				else
				{
					Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
					time = 0f;
					mSyncTime++;
					goto Label_break;
				}
			}
		}
		yield break;
			

	Label_break:
		while ((mTdir.Status() != TdirResult.RecvDone) && ((syncTimeOut == 0) || (time < syncTimeOut)))
		{
			time += Time.deltaTime;
			var innerResult = mTdir.Recv(recvTimeout);
			m_GetinnerResult = innerResult;
			mAsyncFinished = AsyncStatu.IsAsyncing;
			if (innerResult == TdirResult.AllIpConnectFail)
			{
				mAsyncFinished = AsyncStatu.AsyncFail;
				break;
			}
			yield return new WaitForSeconds(0.01f);
		}

		Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
		if (mTdir.Status() == TdirResult.RecvDone)
		{
			if (mTdir.GetTreeNodes(ref mTreeNodes) == TdirResult.TdirNoError)
			{
				mAsyncFinished = AsyncStatu.AsyncSuccess;
				if (mOpenIDTreeNodeDic.ContainsKey(info.OpenId + info.Platform))
				{
					mOpenIDTreeNodeDic[info.OpenId + info.Platform] = mTreeNodes;
				}
				else
				{
					mOpenIDTreeNodeDic.Add(info.OpenId + info.Platform, mTreeNodes);
				}
			}
		}

	Label_03BF:
		if (mAsyncFinished != AsyncStatu.AsyncSuccess)
		{
			mAsyncFinished = AsyncStatu.AsyncFail;
			if (mSyncTime < 3)
			{
				m_GetTdirTime = Time.time;
				MonoSingleton<CTongCaiSys>.instance.isCanUseTongCai = false;
				SetIpAndPort();
				//StartCoroutine(QueryTdirAsync(info, successCallBack, failCallBack, false));
			}
			else
			{
				Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("ChooseSvr_WeakNetwork"), enUIEventID.TDir_TryAgain, enUIEventID.TDir_QuitGame, Singleton<CTextManager>.GetInstance().GetText("ChooseSvr_Retry"), Singleton<CTextManager>.GetInstance().GetText("ChooseSvr_Exit"), false);
			}

			List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>>();
			events.Add(new KeyValuePair<string, string>("g_version", CVersion.GetAppVersion()));
			events.Add(new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()));
			events.Add(new KeyValuePair<string, string>("platform", Singleton<ApolloHelper>.GetInstance().CurPlatform.ToString()));
			events.Add(new KeyValuePair<string, string>("openid", "NULL"));
			float num2 = Time.time - m_GetTdirTime;
			events.Add(new KeyValuePair<string, string>("totaltime", num2.ToString()));
			int errorcode = (int)m_GetinnerResult;
			events.Add(new KeyValuePair<string, string>("errorCode", errorcode.ToString()));
			events.Add(new KeyValuePair<string, string>("error_msg", "null"));
			Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_Login_SelectServer", events, true);
		}
		else
		{
			TdirMgr.mIsCheckVersion = false;
			SetGlobalConfig();
			try
			{
				if (!File.Exists(Application.persistentDataPath + "/ServerListCfg.xml"))
				{
					CheckAuditVersion();
				}
			}
			catch (Exception)
			{
				CheckAuditVersion();
			}
			mCommonData = mTdir.GetTreeCommonData();
			InitTdir(info.Platform);
			mSyncTime = 0;
			if (successCallBack != null)
			{
				successCallBack();
			}
			if (!TdirMgr.IsCheckVersion)
			{
			}
			List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>>();
			events.Add(new KeyValuePair<string, string>("g_version", CVersion.GetAppVersion()));
			events.Add(new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()));
			events.Add(new KeyValuePair<string, string>("platform", Singleton<ApolloHelper>.GetInstance().CurPlatform.ToString()));
			events.Add(new KeyValuePair<string, string>("openid", "NULL"));
			float num3 = Time.time - m_GetTdirTime;
			events.Add(new KeyValuePair<string, string>("totaltime", num3.ToString()));
			int errorcode = (int)m_GetinnerResult;
			events.Add(new KeyValuePair<string, string>("errorCode", errorcode.ToString()));
			events.Add(new KeyValuePair<string, string>("error_msg", "null"));
			Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_Login_SelectServer", events, true);
			Singleton<CLoginSystem>.GetInstance().m_fLoginBeginTime = Time.time;
		}
    }

    private IEnumerator QueryISP()
    {
		ResetISPData();
		var beginTime = Time.time;
		var queryResult = mTdirISP.QueryISP(TdirConfig.GetTdirAppId(), m_iplist, m_portlist, string.Empty, 0, string.Empty);
		while (mTdirISP.Status() != TdirResult.RecvDone)
		{
			if (mTdirISP.Recv(recvTimeout) != TdirResult.AllIpConnectFail) {
				yield return new WaitForSeconds(0.01f);
			}
			else{
				break;
			}
		}

		if (mTdirISP.Status() == TdirResult.RecvDone)
		{
			mCommonData = mTdirISP.GetTreeCommonData();
			var useTime = Time.time - beginTime;
			UnityEngine.Debug.Log("QueryISP use time: " + useTime);
		}
    }

	private void CheckAuditVersion()
	{
		IPAddress iPAddress;
		if (IPAddress.TryParse(GameFramework.AppVersion, out iPAddress))
		{
			int num = BitConverter.ToInt32(iPAddress.GetAddressBytes(), 0);
			object obj = 0;
			for (int i = 0; i < this.mTreeNodes.Count; i++)
			{
				if (this.ParseNodeAppAttr(this.mTreeNodes[i].staticInfo.appAttr, TdirNodeAttrType.versionOnlyExcept, ref obj) && (int)obj == num)
				{
					TdirMgr.mIsCheckVersion = true;
					break;
				}
			}
		}
	}

	private void CheckPreEnable()
	{
		for (int i = 0; i < this.mTreeNodes.Count; i++)
		{
			object obj = null;
			if (this.ParseNodeAppAttr(this.mTreeNodes[i].staticInfo.appAttr, TdirNodeAttrType.PreEnable, ref obj) && !string.IsNullOrEmpty((string)obj))
			{
				this.preVersion = (string)obj;
				this.isPreEnabled = true;
				break;
			}
		}
	}

	private void CheckHttpDns()
	{
		object obj = null;
		this.isUseHttpDns = true;
		if (this.ParseNodeAppAttr(this.mRootNodeAppAttr, TdirNodeAttrType.CloseHttpDns, ref obj) && (int)obj == 1)
		{
			this.isUseHttpDns = false;
		}
	}

	private void ParseNodeInfo()
	{
		if (this.mTreeNodes == null)
		{
			return;
		}
		object obj = new object();
		for (int i = 0; i < this.mTreeNodes.Count; i++)
		{
			if (this.GetIPPosByNodeID(this.mTreeNodes[i].nodeID, 0) == this.mCurrentNodeIDFirst)
			{
				if (this.mTreeNodes[i].parentID == 0)
				{
					this.mRootNodeAppAttr = this.mTreeNodes[i].staticInfo.appAttr;
				}
				TdirUrl tdirUrl = default(TdirUrl);
				if (this.CreateSvrUrl(this.mTreeNodes[i], ref tdirUrl))
				{
					if (tdirUrl.roleCount != 0u)
					{
						this.mOwnRoleList.tdirUrls.Add(tdirUrl);
					}
					if (tdirUrl.flag == SvrFlag.Recommend || (this.ParseNodeAppAttr(this.mTreeNodes[i].staticInfo.appAttr, TdirNodeAttrType.recommond, ref obj) && (int)obj != 0))
					{
						this.mRecommondUrlList.tdirUrls.Add(tdirUrl);
					}
					if (tdirUrl.roleCount > 0u && this.mLastLoginUrl.lastLoginTime < tdirUrl.lastLoginTime)
					{
						this.mLastLoginUrl = tdirUrl;
					}
					if (this.mTreeNodes[i].parentID != 0)
					{
						if (this.mTreeNodes[i].nodeType == 0)
						{
							TdirSvrGroup item = default(TdirSvrGroup);
							if (this.CreateSvrGroup(this.mTreeNodes[i], ref item))
							{
								this.mSvrListGroup.Add(item);
							}
						}
						else
						{
							bool flag = false;
							for (int j = 0; j < this.mSvrListGroup.Count; j++)
							{
								if (this.mTreeNodes[i].parentID == this.mSvrListGroup[j].nodeID)
								{
									this.mSvrListGroup[j].tdirUrls.Add(tdirUrl);
									if (string.IsNullOrEmpty(tdirUrl.originalUrl) && !string.IsNullOrEmpty(this.mSvrListGroup[j].originalUrl))
									{
										tdirUrl.originalUrl = this.mSvrListGroup[j].originalUrl;
										this.ParseTdriConnectUrl(tdirUrl);
									}
									flag = true;
								}
							}
							if (!flag)
							{
								for (int k = 0; k < this.mTreeNodes.Count; k++)
								{
									if (this.mTreeNodes[i].parentID == this.mTreeNodes[k].nodeID)
									{
										TdirSvrGroup item2 = default(TdirSvrGroup);
										if (this.CreateSvrGroup(this.mTreeNodes[k], ref item2))
										{
											item2.tdirUrls.Add(tdirUrl);
											if (string.IsNullOrEmpty(tdirUrl.originalUrl) && !string.IsNullOrEmpty(this.mSvrListGroup[k].originalUrl))
											{
												tdirUrl.originalUrl = this.mSvrListGroup[k].originalUrl;
												this.ParseTdriConnectUrl(tdirUrl);
											}
											this.mSvrListGroup.Add(item2);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void ResetLastLoginUrl()
	{
		this.mLastLoginUrl.logicWorldID = 0;
		this.mLastLoginUrl.nodeID = 0;
		this.mLastLoginUrl.lastLoginTime = 0u;
		this.mLastLoginUrl.name = null;
		this.mLastLoginUrl.flag = SvrFlag.NoFlag;
		this.mLastLoginUrl.attr = null;
		this.mLastLoginUrl.statu = TdirSvrStatu.UNAVAILABLE;
		this.mLastLoginUrl.addrs = null;
	}

	private void ResetSelectedUrl()
	{
		this.mSelectedTdir.logicWorldID = 0;
		this.mSelectedTdir.nodeID = 0;
		this.mSelectedTdir.lastLoginTime = 0u;
		this.mSelectedTdir.name = null;
		this.mSelectedTdir.flag = SvrFlag.NoFlag;
		this.mSelectedTdir.attr = null;
		this.mSelectedTdir.statu = TdirSvrStatu.UNAVAILABLE;
		this.mSelectedTdir.addrs = null;
	}

	private bool CreateSvrGroup(TdirTreeNode node, ref TdirSvrGroup group)
	{
		if (!this.CheckTreeNodeValid(node))
		{
			return false;
		}
		group.nodeID = node.nodeID;
		group.name = node.name;
		group.originalUrl = node.dynamicInfo.connectUrl;
		group.tdirUrls = new List<TdirUrl>();
		return true;
	}

	private bool CreateSvrUrl(TdirTreeNode node, ref TdirUrl tdirUrl)
	{
		string connectUrl = node.dynamicInfo.connectUrl;
		tdirUrl.originalUrl = node.dynamicInfo.connectUrl;
		if (string.IsNullOrEmpty(connectUrl))
		{
			return false;
		}
		if (!this.CheckTreeNodeValid(node))
		{
			return false;
		}
		if ((node.staticInfo.cltAttr & 65536) != 0)
		{
			tdirUrl.isPreSvr = true;
		}
		else
		{
			tdirUrl.isPreSvr = false;
		}
		tdirUrl.nodeID = node.nodeID;
		tdirUrl.name = node.name;
		tdirUrl.statu = (TdirSvrStatu)node.status;
		if (tdirUrl.statu == TdirSvrStatu.FINE || tdirUrl.statu == TdirSvrStatu.HEAVY)
		{
			tdirUrl.flag = (SvrFlag)node.svrFlag;
		}
		else
		{
			tdirUrl.flag = SvrFlag.NoFlag;
		}
		tdirUrl.mask = node.staticInfo.bitmapMask;
		tdirUrl.addrs = new List<IPAddrInfo>();
		tdirUrl.attr = node.staticInfo.appAttr;
		if (node.userRoleInfo != null)
		{
			tdirUrl.roleCount = (uint)node.userRoleInfo.Count;
			uint num = 0u;
			for (int i = 0; i < node.userRoleInfo.Count; i++)
			{
				if (node.userRoleInfo[i].lastLoginTime > num)
				{
					num = node.userRoleInfo[i].lastLoginTime;
				}
			}
			tdirUrl.lastLoginTime = num;
		}
		else
		{
			tdirUrl.roleCount = 0u;
			tdirUrl.lastLoginTime = 0u;
		}
		string[] array = connectUrl.Split(new char[]
		{
			';'
		});
		if (array == null)
		{
			return false;
		}
		for (int j = 0; j < array.Length; j++)
		{
			string[] array2 = array[j].Split(new char[]
			{
				':'
			});
			for (int k = 1; k < array2.Length; k++)
			{
				if (array2[k].IndexOf('-') >= 0)
				{
					string[] array3 = array2[k].Split(new char[]
					{
						'-'
					});
					if (array3.Length == 2)
					{
						int num2;
						if (int.TryParse(array3[0], out num2))
						{
							int num3;
							if (int.TryParse(array3[1], out num3))
							{
								if (num2 <= num3)
								{
									int num4 = num3 - num2 + 1;
									for (int l = 0; l < num4; l++)
									{
										int num5 = num2 + l;
										IPAddrInfo item = default(IPAddrInfo);
										item.ip = array2[0];
										item.port = num5.ToString();
										tdirUrl.addrs.Add(item);
									}
								}
							}
						}
					}
				}
				else
				{
					IPAddrInfo item2 = default(IPAddrInfo);
					item2.ip = array2[0];
					item2.port = array2[k];
					tdirUrl.addrs.Add(item2);
				}
			}
		}
		object obj = null;
		if (this.ParseNodeAppAttr(tdirUrl.attr, TdirNodeAttrType.LogicWorldID, ref obj))
		{
			tdirUrl.logicWorldID = (int)obj;
		}
		else
		{
			tdirUrl.logicWorldID = 0;
		}
		object obj2 = null;
		if (this.ParseNodeAppAttr(tdirUrl.attr, TdirNodeAttrType.UseNetAcc, ref obj2))
		{
			tdirUrl.useNetAcc = (bool)obj2;
		}
		else
		{
			tdirUrl.useNetAcc = false;
		}
		return true;
	}

	private bool ParseTdriConnectUrl(TdirUrl srcUrl)
	{
		string[] array = srcUrl.originalUrl.Split(new char[]
		{
			';'
		});
		if (array == null || array.Length == 0)
		{
			return false;
		}
		srcUrl.addrs.Clear();
		for (int i = 0; i < srcUrl.originalUrl.Length; i++)
		{
			string[] array2 = array[i].Split(new char[]
			{
				':'
			});
			for (int j = 1; j < array2.Length; j++)
			{
				if (array2[j].IndexOf('-') >= 0)
				{
					string[] array3 = array2[j].Split(new char[]
					{
						'-'
					});
					if (array3.Length == 2)
					{
						int num;
						if (int.TryParse(array3[0], out num))
						{
							int num2;
							if (int.TryParse(array3[1], out num2))
							{
								if (num <= num2)
								{
									int num3 = num2 - num + 1;
									for (int k = 0; k < num3; k++)
									{
										int num4 = num + k;
										IPAddrInfo item = default(IPAddrInfo);
										item.ip = array2[0];
										item.port = num4.ToString();
										srcUrl.addrs.Add(item);
									}
								}
							}
						}
					}
				}
				else
				{
					IPAddrInfo item2 = default(IPAddrInfo);
					item2.ip = array2[0];
					item2.port = array2[j];
					srcUrl.addrs.Add(item2);
				}
			}
		}
		return true;
	}

	private bool CreateSvrUrl(TdirUrl srcUrl, ref TdirUrl tdirUrl)
	{
		if (!this.CheckTdirUrlValid(srcUrl))
		{
			tdirUrl.nodeID = 0;
			return false;
		}
		tdirUrl.nodeID = srcUrl.nodeID;
		tdirUrl.name = srcUrl.name;
		tdirUrl.statu = srcUrl.statu;
		tdirUrl.flag = srcUrl.flag;
		tdirUrl.mask = srcUrl.mask;
		tdirUrl.roleCount = srcUrl.roleCount;
		tdirUrl.attr = srcUrl.attr;
		tdirUrl.addrs = new List<IPAddrInfo>(srcUrl.addrs);
		tdirUrl.logicWorldID = srcUrl.logicWorldID;
		return true;
	}

	public bool GetTreeNodeAttr(TdirTreeNode node, TdirNodeAttrType attrType, ref object param)
	{
		return this.ParseNodeAppAttr(node.staticInfo.appAttr, attrType, ref param);
	}

	public bool ParseNodeAppAttr(string attr, TdirNodeAttrType attrType, ref object param)
	{
		if (!string.IsNullOrEmpty(attr))
		{
			attr = attr.ToLower();
			string[] array = attr.Split(new char[]
			{
				';'
			});
			if (array == null)
			{
				return false;
			}
			string b = attrType.ToString().ToLower();
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(new char[]
				{
					':'
				});
				if (array2 != null && array2.Length == 2 && !string.IsNullOrEmpty(array2[0]) && !string.IsNullOrEmpty(array2[1]) && array2[0].ToLower() == b)
				{
					switch (attrType)
					{
					case TdirNodeAttrType.recommond:
					case TdirNodeAttrType.enableTSS:
					case TdirNodeAttrType.backTime:
					case TdirNodeAttrType.msdkLogMem:
					case TdirNodeAttrType.tdirTimeOut:
					case TdirNodeAttrType.tdirSyncTimeOut:
					case TdirNodeAttrType.waitHurtActionDone:
					case TdirNodeAttrType.unloadValidCnt:
					case TdirNodeAttrType.checkdevice:
					case TdirNodeAttrType.authorParam:
					case TdirNodeAttrType.autoReplaceActorParam:
					case TdirNodeAttrType.socketRecvBuffer:
					case TdirNodeAttrType.reconnectMaxCnt:
					case TdirNodeAttrType.LogicWorldID:
					case TdirNodeAttrType.UseDeviceIDChooseSvr:
					case TdirNodeAttrType.IOSX:
					case TdirNodeAttrType.UseTongcai:
					case TdirNodeAttrType.ShowTCBtn:
					case TdirNodeAttrType.CloseHttpDns:
					{
						int num = 0;
						if (int.TryParse(array2[1], out num))
						{
							param = num;
							return true;
						}
						break;
					}
					case TdirNodeAttrType.ISPChoose:
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>();
						string[] array3 = array2[1].Split(new char[]
						{
							'*'
						});
						for (int j = 0; j < array3.Length; j++)
						{
							string[] array4 = array3[j].Split(new char[]
							{
								'$'
							});
							DebugHelper.Assert(array4.Length == 2, "后台大爷把用于ISP解析的字符串配错了");
							int value = 0;
							int.TryParse(array4[0], out value);
							IPAddress iPAddress;
							if (!IPAddress.TryParse(array4[1], out iPAddress))
							{
								DebugHelper.Assert(false, "后台大爷把用于ISP解析的字符串配错了,ip解析不了");
							}
							dictionary.Add(array4[1], value);
						}
						param = dictionary;
						return true;
					}
					case TdirNodeAttrType.versionDown:
					case TdirNodeAttrType.versionUp:
					case TdirNodeAttrType.versionOnlyExcept:
					{
						IPAddress iPAddress2;
						if (IPAddress.TryParse(array2[1], out iPAddress2))
						{
							param = BitConverter.ToInt32(iPAddress2.GetAddressBytes(), 0);
							return true;
						}
						break;
					}
					case TdirNodeAttrType.UseNetAcc:
					{
						int num2 = 0;
						if (int.TryParse(array2[1], out num2))
						{
							if (num2 != 0)
							{
								param = true;
							}
							else
							{
								param = false;
							}
							return true;
						}
						break;
					}
					case TdirNodeAttrType.PreEnable:
						param = array2[1];
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool ParseNodeAppAttrWithBackup(string attr, string bacbupAttr, TdirNodeAttrType attrType, ref object param)
	{
		bool flag = this.ParseNodeAppAttr(attr, attrType, ref param);
		if (!flag)
		{
			flag = this.ParseNodeAppAttr(bacbupAttr, attrType, ref param);
		}
		return flag;
	}

	private void SetGlobalConfig()
	{
		if (this.mTreeNodes == null || this.mTreeNodes.Count == 0)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < this.mTreeNodes.Count; i++)
		{
			object obj = new object();
			if (this.GetTreeNodeAttr(this.mTreeNodes[i], TdirNodeAttrType.enableTSS, ref obj))
			{
				PlayerPrefs.SetInt("EnableTSS", (int)obj);
				flag = true;
			}
			if (!flag2 && this.GetTreeNodeAttr(this.mTreeNodes[i], TdirNodeAttrType.tdirTimeOut, ref obj))
			{
				PlayerPrefs.SetInt(TdirNodeAttrType.tdirTimeOut.ToString(), (int)obj);
				flag2 = true;
				this.apolloTimeout = (int)obj;
			}
			if (this.GetTreeNodeAttr(this.mTreeNodes[i], TdirNodeAttrType.tdirSyncTimeOut, ref obj))
			{
				PlayerPrefs.SetInt(TdirNodeAttrType.tdirSyncTimeOut.ToString(), (int)obj);
				flag3 = true;
				this.syncTimeOut = (int)obj;
			}
			if (flag && flag2 && flag3)
			{
				return;
			}
		}
	}

	private void SetConfigAtClickTdir(TdirUrl tdirUrl)
	{
		if (tdirUrl.nodeID != 0)
		{
			object obj = null;
			if (!this.ParseNodeAppAttr(tdirUrl.attr, TdirNodeAttrType.reconnectMaxCnt, ref obj) || (int)obj >= 0)
			{
			}
		}
	}

	private int GetIPPosByNodeID(int nodeID, int pos)
	{
		return nodeID & 255 << pos * 8;
	}

	private int GetNodeIDByPos(int first = 0, int second = 0, int third = 0, int forth = 0)
	{
		if (first == 0 && second == 0 && third == 0 && forth == 0)
		{
			return 0;
		}
		return first | second << 8 | third << 16 | forth << 24;
	}

	public void ChooseGameServer(TdirUrl tdirurl)
	{
		this.m_connectIndex = -1;
		if (this.mCurrentNodeIDFirst != TdirMgr.InternalNodeID && tdirurl.statu == TdirSvrStatu.UNAVAILABLE && TdirMgr.s_maintainBlock)
		{
			Singleton<CUIManager>.GetInstance().OpenMessageBox(Singleton<CTextManager>.GetInstance().GetText("Maintaining"), false);
			return;
		}
		this.mSelectedTdir = tdirurl;
		if (tdirurl.addrs == null || tdirurl.addrs.Count <= 0)
		{
			DebugHelper.Assert(false, "{0} 后台大爷给这个区配个gameserver的url呗", new object[]
			{
				tdirurl.name
			});
			return;
		}
		MonoSingleton<CTongCaiSys>.instance.isCanUseTongCai = true;
		object obj = null;
		if (this.ParseNodeAppAttr(this.SelectedTdir.attr, TdirNodeAttrType.UseTongcai, ref obj) && (int)obj == 0)
		{
			MonoSingleton<CTongCaiSys>.instance.isCanUseTongCai = false;
		}
		Singleton<ReconnectIpSelect>.instance.Reset();
		if (MonoSingleton<CVersionUpdateSystem>.GetInstance().IsEnablePreviousVersion())
		{
			bool flag = MonoSingleton<CVersionUpdateSystem>.GetInstance().IsPreviousApp();
			bool isPreSvr = this.mSelectedTdir.isPreSvr;
			if (!flag && isPreSvr)
			{
				string strContent = (ApolloConfig.platform != ApolloPlatform.Wechat) ? Singleton<CTextManager>.GetInstance().GetCombineText("UpdateToPreviousVersionNoticeQQ1", "UpdateToPreviousVersionNoticeQQ2") : Singleton<CTextManager>.GetInstance().GetCombineText("UpdateToPreviousVersionNoticeWX1", "UpdateToPreviousVersionNoticeWX2");
				Singleton<CUIManager>.GetInstance().OpenMessageBoxBig(strContent, true, enUIEventID.VersionUpdate_UpdateToPreviousVersion, enUIEventID.None, default(stUIEventParams), false, string.Empty, string.Empty, Singleton<CTextManager>.instance.GetText("NewServerTitleName1"), 0, enUIEventID.None);
				return;
			}
			if (flag && !isPreSvr)
			{
				Singleton<CUIManager>.GetInstance().OpenMessageBoxBig(Singleton<CTextManager>.GetInstance().GetText("PreviousVersionCanNotLoginNotice"), false, enUIEventID.None, enUIEventID.None, default(stUIEventParams), false, string.Empty, string.Empty, Singleton<CTextManager>.instance.GetText("NewServerTitleName2"), 0, enUIEventID.None);
				return;
			}
		}
		if (this.mSelectedTdir.isPreSvr && this.mSelectedTdir.roleCount <= 0u)
		{
			string strContent2 = (ApolloConfig.platform != ApolloPlatform.Wechat) ? Singleton<CTextManager>.GetInstance().GetCombineText("PreviousServerRegisterConfirmQQ1", "PreviousServerRegisterConfirmQQ2", "PreviousServerRegisterConfirmQQ3", "PreviousServerRegisterConfirmQQ4") : Singleton<CTextManager>.GetInstance().GetCombineText("PreviousServerRegisterConfirmWX1", "PreviousServerRegisterConfirmWX2", "PreviousServerRegisterConfirmWX3", "PreviousServerRegisterConfirmWX4");
			Singleton<CUIManager>.GetInstance().OpenMessageBoxBig(strContent2, true, enUIEventID.TDir_ConnectLobby, enUIEventID.None, default(stUIEventParams), false, string.Empty, string.Empty, Singleton<CTextManager>.instance.GetText("NewServerTitleName3"), 0, enUIEventID.None);
			return;
		}
		this.ConnectLobby();
	}

	public void StartGetHostAddress(TdirUrl tdirurl)
	{
		bool flag = false;
		IPAddress iPAddress;
		if (IPAddress.TryParse(tdirurl.addrs[0].ip, out iPAddress))
		{
			long num = (long)BitConverter.ToInt32(iPAddress.GetAddressBytes(), 0);
			if (num > 0L)
			{
				flag = true;
			}
		}
		ApolloConfig.ISPType = 0;
		if (!flag)
		{
			if (this.getAddressResult == null)
			{
				this.getAddressResult = Dns.BeginGetHostAddresses(tdirurl.addrs[0].ip, null, null);
			}
			if (this.getAddressResult == null)
			{
				object obj = 0;
				if (this.ParseNodeAppAttrWithBackup(tdirurl.attr, this.mRootNodeAppAttr, TdirNodeAttrType.ISPChoose, ref obj))
				{
					Dictionary<string, int> dictionary = (Dictionary<string, int>)obj;
					if (dictionary != null)
					{
						foreach (KeyValuePair<string, int> current in dictionary)
						{
							if (current.Value == (int)this.GetISP())
							{
								ApolloConfig.loginUrl = string.Format("tcp://{0}:{1}", current.Key, tdirurl.addrs[0].port);
								ApolloConfig.loginOnlyIpOrUrl = current.Key;
								ApolloConfig.loginOnlyVPort = ushort.Parse(tdirurl.addrs[0].port);
								ApolloConfig.ISPType = (int)this.GetISP();
								break;
							}
						}
					}
				}
			}
		}
		else
		{
			ApolloConfig.loginUrl = string.Format("tcp://{0}:{1}", tdirurl.addrs[0].ip, tdirurl.addrs[0].port);
			ApolloConfig.loginOnlyIpOrUrl = tdirurl.addrs[0].ip;
			ApolloConfig.loginOnlyVPort = ushort.Parse(tdirurl.addrs[0].port);
			ApolloConfig.ISPType = 1;
		}
	}

	public void ConnectLobby()
	{
		if (this.m_connectIndex < 0)
		{
			int num = 1;
			object obj = null;
			if (this.ParseNodeAppAttr(this.SelectedTdir.attr, TdirNodeAttrType.UseDeviceIDChooseSvr, ref obj))
			{
				num = (int)obj;
			}
			if (num == 1 && !string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier))
			{
				byte[] bytes = Encoding.ASCII.GetBytes(SystemInfo.deviceUniqueIdentifier);
				MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
				mD5CryptoServiceProvider.Initialize();
				mD5CryptoServiceProvider.TransformFinalBlock(bytes, 0, bytes.Length);
				ulong num2 = (ulong)BitConverter.ToInt64(mD5CryptoServiceProvider.Hash, 0);
				ulong num3 = (ulong)BitConverter.ToInt64(mD5CryptoServiceProvider.Hash, 8);
				this.m_connectIndex = (int)(num2 ^ num3);
				if (this.m_connectIndex < 0)
				{
					this.m_connectIndex *= -1;
				}
			}
			else
			{
				this.m_connectIndex = UnityEngine.Random.Range(0, 10000);
			}
			this.m_connectIndex %= MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs.Count;
			Singleton<LobbySvrMgr>.GetInstance().ConnectServerWithTdirDefault(this.m_connectIndex, ChooseSvrPolicy.DeviceID);
		}
		else
		{
			this.m_connectIndex++;
			this.m_connectIndex %= MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs.Count;
			Singleton<LobbySvrMgr>.GetInstance().ConnectServerWithTdirDefault(this.m_connectIndex, Singleton<LobbySvrMgr>.GetInstance().chooseSvrPol);
		}
	}

	public void ConnectLobbyWithSnsNameChooseSvr()
	{
		if (!string.IsNullOrEmpty(Singleton<CLoginSystem>.GetInstance().m_nickName))
		{
			byte[] bytes = Encoding.ASCII.GetBytes(Singleton<CLoginSystem>.GetInstance().m_nickName);
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			mD5CryptoServiceProvider.Initialize();
			mD5CryptoServiceProvider.TransformFinalBlock(bytes, 0, bytes.Length);
			ulong num = (ulong)BitConverter.ToInt64(mD5CryptoServiceProvider.Hash, 0);
			ulong num2 = (ulong)BitConverter.ToInt64(mD5CryptoServiceProvider.Hash, 8);
			this.m_connectIndex = (int)(num ^ num2);
			if (this.m_connectIndex < 0)
			{
				this.m_connectIndex *= -1;
			}
		}
		else
		{
			this.m_connectIndex = UnityEngine.Random.Range(0, 10000);
		}
		this.m_connectIndex %= MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs.Count;
		Singleton<LobbySvrMgr>.GetInstance().ConnectServerWithTdirDefault(this.m_connectIndex, ChooseSvrPolicy.NickName);
	}

	public void ConnectLobbyRandomChooseSvr(ChooseSvrPolicy policy)
	{
		this.m_connectIndex = UnityEngine.Random.Range(0, 10000);
		this.m_connectIndex %= MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs.Count;
		Singleton<LobbySvrMgr>.GetInstance().ConnectServerWithTdirDefault(this.m_connectIndex, policy);
	}

	private void Update()
	{
	}

	public bool ShowBuyTongCaiBtn()
	{
		object obj = null;
		if (this.ParseNodeAppAttr(this.SelectedTdir.attr, TdirNodeAttrType.ShowTCBtn, ref obj))
		{
			int num = (int)obj;
			if (num == 1)
			{
				return true;
			}
		}
		return false;
	}

	public IspType GetISP()
	{
		if (this.mCommonData.ispCode <= 0)
		{
			return IspType.Null;
		}
		if (this.mCommonData.ispCode == 1)
		{
			return IspType.Dianxing;
		}
		if (this.mCommonData.ispCode == 2)
		{
			return IspType.Liantong;
		}
		if (this.mCommonData.ispCode == 5)
		{
			return IspType.Yidong;
		}
		return IspType.Other;
	}

	public void ResetISPData()
	{
		this.mCommonData.ispCode = -1;
		this.mCommonData.provinceCode = -1;
	}

	public string GetDianXingIP()
	{
		return this.GetIpByIspCode(1);
	}

	public string GetLianTongIP()
	{
		return this.GetIpByIspCode(2);
	}

	public string GetYiDongIP()
	{
		return this.GetIpByIspCode(3);
	}

	public string GetIpByIspCode(int code)
	{
		object obj = 0;
		if (this.ParseNodeAppAttrWithBackup(this.SelectedTdir.attr, this.mRootNodeAppAttr, TdirNodeAttrType.ISPChoose, ref obj))
		{
			Dictionary<string, int> dictionary = (Dictionary<string, int>)obj;
			if (dictionary != null)
			{
				foreach (KeyValuePair<string, int> current in dictionary)
				{
					if (current.Value == code)
					{
						return current.Key;
					}
				}
			}
		}
		return null;
	}
}
