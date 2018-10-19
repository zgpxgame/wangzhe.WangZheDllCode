using Assets.Scripts.Common;
using Mono.Xml;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using UnityEngine;

namespace AGE
{
	public class ActionManager : MonoSingleton<ActionManager>
	{
		public bool debugMode;

		public bool frameMode;

		public bool isPrintLog;

		private ListView<Action> conflictActionsToStop = new ListView<Action>();

		public ListView<Action> actionList = new ListView<Action>();

		public DictionaryView<string, Action> actionResourceSet = new DictionaryView<string, Action>();

		public DictionaryView<GameObject, ListView<Action>> objectReferenceSet = new DictionaryView<GameObject, ListView<Action>>();

		public DictionaryView<string, ActionCommonData> actionCommonDataSet = new DictionaryView<string, ActionCommonData>();

		private ListView<Action> actionUpdatingList = new ListView<Action>();

		private List<PoolObjHandle<Action>> waitReleaseList = new List<PoolObjHandle<Action>>();

		private AssetLoader resLoader;

		public string assetLoaderClass = string.Empty;

		public string[] preloadActions = new string[0];

		public bool preloadActionHelpers;

		public bool preloadResources;

		public AssetLoader ResLoader
		{
			get
			{
				return this.resLoader;
			}
		}

		public int ActionUpdatingCount
		{
			get
			{
				return this.actionUpdatingList.Count;
			}
		}

		public static ActionManager Instance
		{
			get
			{
				return MonoSingleton<ActionManager>.instance;
			}
		}

		public static void DestroyGameObject(UnityEngine.Object obj)
		{
			if (ActionManager.Instance != null && ActionManager.Instance.ResLoader != null)
			{
				ActionManager.Instance.ResLoader.DestroyObject(obj);
				return;
			}
			if (obj is GameObject)
			{
				Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(obj as GameObject);
				return;
			}
			UnityEngine.Object.Destroy(obj);
		}

		public static void DestroyGameObjectFromAction(Action action, GameObject obj)
		{
			ActionManager.DestroyGameObject(obj);
			action.ClearGameObject(obj);
		}

		public static UnityEngine.Object InstantiateObject(UnityEngine.Object prefab)
		{
			if (ActionManager.Instance != null && ActionManager.Instance.ResLoader != null)
			{
				return ActionManager.Instance.ResLoader.Instantiate(prefab);
			}
			return UnityEngine.Object.Instantiate(prefab);
		}

		public static UnityEngine.Object InstantiateObject(UnityEngine.Object prefab, Vector3 pos, Quaternion rot)
		{
			if (ActionManager.Instance != null && ActionManager.Instance.ResLoader != null)
			{
				return ActionManager.Instance.ResLoader.Instantiate(prefab, pos, rot);
			}
			return UnityEngine.Object.Instantiate(prefab, pos, rot);
		}

		private void Initialize()
		{
			foreach (Transform transform in base.transform)
			{
				ActionManager.DestroyGameObject(transform.gameObject);
			}
			if (this.resLoader == null)
			{
				Type type = Utility.GetType(this.assetLoaderClass);
				if (type != null)
				{
					this.resLoader = (Activator.CreateInstance(type) as AssetLoader);
				}
				if (this.resLoader == null)
				{
					this.resLoader = new AssetLoader();
				}
			}
			this.resLoader.preloading = true;
			string[] array = this.preloadActions;
			for (int i = 0; i < array.Length; i++)
			{
				string actionName = array[i];
				this.LoadActionResource(actionName);
			}
			if (this.preloadActionHelpers)
			{
				ActionHelper[] array2 = UnityEngine.Object.FindObjectsOfType<ActionHelper>();
				ActionHelper[] array3 = array2;
				for (int j = 0; j < array3.Length; j++)
				{
					ActionHelper actionHelper = array3[j];
					ActionHelperStorage[] actionHelpers = actionHelper.actionHelpers;
					for (int k = 0; k < actionHelpers.Length; k++)
					{
						ActionHelperStorage actionHelperStorage = actionHelpers[k];
						this.LoadActionResource(actionHelperStorage.actionName);
					}
				}
			}
			if (this.preloadResources)
			{
			}
			this.resLoader.preloading = false;
		}

		public void ForceStart()
		{
			this.Initialize();
		}

		public void ForceStop()
		{
			if (MonoSingleton<ActionManager>.instance == null)
			{
				return;
			}
			this.StopAllActions();
			this.actionList.Clear();
			this.actionResourceSet.Clear();
			this.objectReferenceSet.Clear();
			foreach (Transform transform in base.transform)
			{
				ActionManager.DestroyGameObject(transform.gameObject);
			}
		}

		protected override void Init()
		{
			this.Initialize();
		}

		public void ForceReloadAction(string _actionName)
		{
		}

		private void Update()
		{
		}

		public void StopAllActions()
		{
			if (this.actionList.Count == 0)
			{
				return;
			}
			ListView<Action> listView = new ListView<Action>(this.actionList);
			for (int i = 0; i < listView.Count; i++)
			{
				Action action = listView[i];
				action.Stop(true);
			}
		}

		public void UpdateLogic(int nDelta)
		{
			if (!this.frameMode)
			{
				return;
			}
			for (int i = 0; i < this.waitReleaseList.Count; i++)
			{
				if (this.waitReleaseList[i])
				{
					this.waitReleaseList[i].handle.Stop(true);
				}
			}
			this.waitReleaseList.Clear();
			this.actionUpdatingList.Clear();
			int count = this.actionList.Count;
			for (int j = 0; j < count; j++)
			{
				this.actionUpdatingList.Add(this.actionList[j]);
			}
			count = this.actionUpdatingList.Count;
			for (int k = 0; k < count; k++)
			{
				if (!this.actionUpdatingList[k].nextDestroy)
				{
					this.actionUpdatingList[k].UpdateLogic(nDelta);
				}
			}
			for (int l = 0; l < this.waitReleaseList.Count; l++)
			{
				if (this.waitReleaseList[l])
				{
					this.waitReleaseList[l].handle.Stop(true);
				}
			}
			this.waitReleaseList.Clear();
		}

		private Action InternalPlayAction(string _actionName, bool _autoPlay, bool _stopConflictAction, GameObject[] _gameObjects)
		{
			GameObject gameObject = null;
			for (int i = 0; i < _gameObjects.Length; i++)
			{
				GameObject gameObject2 = _gameObjects[i];
				if (!(gameObject2 == null))
				{
					if (this.objectReferenceSet.ContainsKey(gameObject2))
					{
						gameObject = gameObject2;
						break;
					}
				}
			}
			if (gameObject && _stopConflictAction)
			{
				this.conflictActionsToStop.Clear();
				ListView<Action> listView = this.objectReferenceSet[gameObject];
				ListView<Action>.Enumerator enumerator = listView.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Action current = enumerator.Current;
					if (!current.unstoppable)
					{
						this.conflictActionsToStop.Add(current);
					}
				}
				ListView<Action>.Enumerator enumerator2 = this.conflictActionsToStop.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Action current2 = enumerator2.Current;
					current2.Stop(false);
				}
			}
			Action action = this.LoadActionResource(_actionName);
			if (action == null)
			{
				Debug.LogError("Playing \"" + _actionName + "\" failed. Asset not found!");
				return null;
			}
			Action action2 = ClassObjPool<Action>.Get();
			action2.enabled = _autoPlay;
			action2.refGameObjectsCount = _gameObjects.Length;
			action2.LoadAction(action, _gameObjects);
			this.actionList.Add(action2);
			for (int j = 0; j < _gameObjects.Length; j++)
			{
				GameObject gameObject3 = _gameObjects[j];
				if (!(gameObject3 == null))
				{
					ListView<Action> listView2 = null;
					if (this.objectReferenceSet.TryGetValue(gameObject3, out listView2))
					{
						listView2.Add(action2);
					}
					else
					{
						listView2 = new ListView<Action>();
						listView2.Add(action2);
						this.objectReferenceSet.Add(gameObject3, listView2);
					}
				}
			}
			return action2;
		}

		public Action PlayAction(string _actionName, bool _autoPlay, bool _stopConflictAction, params GameObject[] _gameObjects)
		{
			return this.InternalPlayAction(_actionName, _autoPlay, _stopConflictAction, _gameObjects);
		}

		private Action InternalPlaySubAction(Action _parentAction, string _actionName, float _length, GameObject[] _gameObjects)
		{
			Action action = this.LoadActionResource(_actionName);
			if (action == null)
			{
				Debug.LogError("Playing \"" + _actionName + "\" failed. Asset not found!");
				return null;
			}
			Action action2 = ClassObjPool<Action>.Get();
			action2.LoadAction(action, _gameObjects);
			action2.loop = false;
			action2.length = ActionUtility.SecToMs(_length);
			action2.parentAction = _parentAction;
			this.actionList.Add(action2);
			return action2;
		}

		public Action PlaySubAction(Action _parentAction, string _actionName, float _length, params GameObject[] _gameObjects)
		{
			return this.InternalPlaySubAction(_parentAction, _actionName, _length, _gameObjects);
		}

		public void StopAction(Action _action)
		{
			if (_action != null)
			{
				_action.Stop(true);
			}
		}

		public void DestroyObject(GameObject _gameObject)
		{
			ListView<Action> listView = null;
			if (this.objectReferenceSet.TryGetValue(_gameObject, out listView) && listView != null)
			{
				for (int i = 0; i < listView.Count; i++)
				{
					listView[i].ClearGameObject(_gameObject);
				}
			}
			ActionManager.DestroyGameObject(_gameObject);
		}

		public void RemoveAction(Action _action)
		{
			if (_action == null)
			{
				return;
			}
			ListLinqView<GameObject> gameObjectList = _action.GetGameObjectList();
			int count = gameObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = gameObjectList[i];
				if (!(gameObject == null))
				{
					if (this.objectReferenceSet.ContainsKey(gameObject))
					{
						ListView<Action> listView = this.objectReferenceSet[gameObject];
						listView.Remove(_action);
						if (listView.Count == 0)
						{
							this.objectReferenceSet.Remove(gameObject);
						}
					}
					else if (gameObjectList.IndexOf(gameObject) >= _action.refGameObjectsCount)
					{
						ActionManager.DestroyGameObject(gameObject);
					}
				}
			}
			MonoSingleton<ActionManager>.instance.actionList.Remove(_action);
			_action.Release();
		}

		public void DeferReleaseAction(Action _action)
		{
			MonoSingleton<ActionManager>.instance.waitReleaseList.Add(new PoolObjHandle<Action>(_action));
		}

		public bool IsActionValid(Action _action)
		{
			return _action != null && this.actionList.Contains(_action);
		}

		public Dictionary<string, int> LoadTemplateObjectList(Action action)
		{
			string actionName = action.actionName;
			CBinaryObject cBinaryObject = ActionManager.Instance.resLoader.LoadAge(actionName) as CBinaryObject;
			if (cBinaryObject == null)
			{
				return new Dictionary<string, int>();
			}
			SecurityParser securityParser = new SecurityParser();
			securityParser.LoadXml(Encoding.UTF8.GetString(cBinaryObject.m_data));
			Singleton<CResourceManager>.GetInstance().RemoveCachedResource(actionName);
			SecurityElement securityElement = securityParser.SelectSingleNode("Project");
			SecurityElement securityElement2 = (securityElement == null) ? null : securityElement.SearchForChildByTag("TemplateObjectList");
			if (securityElement2 != null)
			{
				action.templateObjectIds.Clear();
				if (securityElement2.Children != null)
				{
					for (int i = 0; i < securityElement2.Children.Count; i++)
					{
						SecurityElement securityElement3 = securityElement2.Children[i] as SecurityElement;
						string key = securityElement3.Attribute("objectName");
						string s = securityElement3.Attribute("id");
						int value = int.Parse(s);
						string a = securityElement3.Attribute("isTemp");
						if (a == "false")
						{
							action.templateObjectIds.Add(key, value);
						}
					}
				}
			}
			return action.templateObjectIds;
		}

		public Action LoadActionResource(string _actionName)
		{
			Action action = null;
			if (_actionName == null)
			{
				DebugHelper.Assert(_actionName != null, "can't load action with name = null");
				return null;
			}
			if (this.actionResourceSet.TryGetValue(_actionName, out action))
			{
				if (action != null)
				{
					return action;
				}
				this.actionResourceSet.Remove(_actionName);
			}
			CBinaryObject cBinaryObject = ActionManager.Instance.resLoader.LoadAge(_actionName) as CBinaryObject;
			if (cBinaryObject == null)
			{
				return null;
			}
			SecurityParser securityParser = new SecurityParser();
			try
			{
				securityParser.LoadXml(Encoding.UTF8.GetString(cBinaryObject.m_data));
			}
			catch (Exception ex)
			{
				DebugHelper.Assert(false, "Load xml Exception for action name = {0}, exception = {1}", new object[]
				{
					_actionName,
					ex.Message
				});
				return null;
			}
			action = new Action();
			action.name = _actionName;
			action.enabled = false;
			action.actionName = _actionName;
			this.actionResourceSet.Add(_actionName, action);
			Singleton<CResourceManager>.GetInstance().RemoveCachedResource(_actionName);
			SecurityElement securityElement = securityParser.SelectSingleNode("Project");
			SecurityElement securityElement2 = (securityElement == null) ? null : securityElement.SearchForChildByTag("TemplateObjectList");
			SecurityElement securityElement3 = (securityElement == null) ? null : securityElement.SearchForChildByTag("Action");
			SecurityElement securityElement4 = (securityElement == null) ? null : securityElement.SearchForChildByTag("RefParamList");
			DebugHelper.Assert(securityElement3 != null, "actionNode!=null");
			if (securityElement3 != null)
			{
				action.length = ActionUtility.SecToMs(float.Parse(securityElement3.Attribute("length")));
				action.loop = bool.Parse(securityElement3.Attribute("loop"));
			}
			if (securityElement2 != null && securityElement2.Children != null)
			{
				for (int i = 0; i < securityElement2.Children.Count; i++)
				{
					SecurityElement securityElement5 = securityElement2.Children[i] as SecurityElement;
					string str = securityElement5.Attribute("objectName");
					string s = securityElement5.Attribute("id");
					int id = int.Parse(s);
					action.AddTemplateObject(str, id);
				}
			}
			if (securityElement4 != null && securityElement4.Children != null)
			{
				for (int j = 0; j < securityElement4.Children.Count; j++)
				{
					this.LoadRefParamNode(action, securityElement4.Children[j] as SecurityElement);
				}
			}
			if (securityElement3 != null && securityElement3.Children != null)
			{
				for (int k = 0; k < securityElement3.Children.Count; k++)
				{
					SecurityElement securityElement6 = securityElement3.Children[k] as SecurityElement;
					string text = securityElement6.Attribute("eventType");
					if (!text.Contains(".") && text.Length > 0)
					{
						text = "AGE." + text;
					}
					Type type = Utility.GetType(text);
					if (type != null)
					{
						string name = string.Empty;
						bool flag = false;
						if (securityElement6.Attribute("refParamName") != null)
						{
							name = securityElement6.Attribute("refParamName");
						}
						if (securityElement6.Attribute("useRefParam") != null)
						{
							flag = bool.Parse(securityElement6.Attribute("useRefParam"));
						}
						bool enabled = bool.Parse(securityElement6.Attribute("enabled"));
						if (flag)
						{
							action.refParams.GetRefParam(name, ref enabled);
						}
						Track track = action.AddTrack(type);
						track.enabled = enabled;
						track.trackName = securityElement6.Attribute("trackName");
						if (securityElement6.Attribute("execOnActionCompleted") != null)
						{
							track.execOnActionCompleted = bool.Parse(securityElement6.Attribute("execOnActionCompleted"));
						}
						if (securityElement6.Attribute("execOnForceStopped") != null)
						{
							track.execOnForceStopped = bool.Parse(securityElement6.Attribute("execOnForceStopped"));
						}
						if (flag)
						{
							FieldInfo field = type.GetField(securityElement6.Attribute("enabled"));
							action.refParams.AddRefData(name, field, track);
						}
						if (securityElement6.Attribute("r") != null)
						{
							track.color.r = float.Parse(securityElement6.Attribute("r"));
						}
						if (securityElement6.Attribute("g") != null)
						{
							track.color.g = float.Parse(securityElement6.Attribute("g"));
						}
						if (securityElement6.Attribute("b") != null)
						{
							track.color.b = float.Parse(securityElement6.Attribute("b"));
						}
						ListView<SecurityElement> listView = new ListView<SecurityElement>();
						if (securityElement6.Children != null)
						{
							for (int l = 0; l < securityElement6.Children.Count; l++)
							{
								SecurityElement securityElement7 = securityElement6.Children[l] as SecurityElement;
								if (securityElement7.Tag != "Event" && securityElement7.Tag != "Condition")
								{
									listView.Add(securityElement7);
								}
							}
							for (int m = 0; m < securityElement6.Children.Count; m++)
							{
								SecurityElement securityElement8 = securityElement6.Children[m] as SecurityElement;
								if (securityElement8.Tag == "Condition")
								{
									SecurityElement securityElement9 = securityElement8;
									int key = int.Parse(securityElement9.Attribute("id"));
									bool value = bool.Parse(securityElement9.Attribute("status"));
									if (track.waitForConditions == null)
									{
										track.waitForConditions = new Dictionary<int, bool>();
									}
									track.waitForConditions.Add(key, value);
								}
								else if (!(securityElement8.Tag != "Event"))
								{
									int time = ActionUtility.SecToMs(float.Parse(securityElement8.Attribute("time")));
									int length = 0;
									if (track.IsDurationEvent)
									{
										length = ActionUtility.SecToMs(float.Parse(securityElement8.Attribute("length")));
									}
									BaseEvent baseEvent = track.AddEvent(time, length);
									for (int n = 0; n < listView.Count; n++)
									{
										this.SetEventField(action, baseEvent, listView[n]);
									}
									if (securityElement8.Children != null)
									{
										for (int num = 0; num < securityElement8.Children.Count; num++)
										{
											this.SetEventField(action, baseEvent, securityElement8.Children[num] as SecurityElement);
										}
									}
									baseEvent.OnLoaded();
								}
							}
						}
					}
					else
					{
						Debug.LogError("Invalid event type \"" + securityElement6.Attribute("eventType") + "\"!");
					}
				}
			}
			return action;
		}

		private void LoadRefParamNode(Action result, SecurityElement paramNode)
		{
			string a = paramNode.Tag.ToLower();
			string name = paramNode.Attribute("name");
			if (a == "float")
			{
				float value = float.Parse(paramNode.Attribute("value"));
				result.refParams.AddRefParam(name, value);
			}
			else if (a == "int")
			{
				int value2 = int.Parse(paramNode.Attribute("value"));
				result.refParams.AddRefParam(name, value2);
			}
			else if (a == "templateobject")
			{
				int value3 = int.Parse(paramNode.Attribute("id"));
				result.refParams.AddRefParam(name, value3);
			}
			else if (a == "uint")
			{
				uint value4 = uint.Parse(paramNode.Attribute("value"));
				result.refParams.AddRefParam(name, value4);
			}
			else if (a == "bool")
			{
				bool value5 = bool.Parse(paramNode.Attribute("value"));
				result.refParams.AddRefParam(name, value5);
			}
			else if (a == "string")
			{
				string value6 = paramNode.Attribute("value");
				result.refParams.AddRefParam(name, value6);
			}
			else if (a == "vector3")
			{
				float x = float.Parse(paramNode.Attribute("x"));
				float y = float.Parse(paramNode.Attribute("y"));
				float z = float.Parse(paramNode.Attribute("z"));
				Vector3 value7 = new Vector3(x, y, z);
				result.refParams.AddRefParam(name, value7);
			}
			else if (a == "vector3i")
			{
				int x2 = int.Parse(paramNode.Attribute("x"));
				int y2 = int.Parse(paramNode.Attribute("y"));
				int z2 = int.Parse(paramNode.Attribute("z"));
				VInt3 value8 = new VInt3(x2, y2, z2);
				result.refParams.AddRefParam(name, value8);
			}
			else if (a == "quaternion")
			{
				float x3 = float.Parse(paramNode.Attribute("x"));
				float y3 = float.Parse(paramNode.Attribute("y"));
				float z3 = float.Parse(paramNode.Attribute("z"));
				float w = float.Parse(paramNode.Attribute("w"));
				Quaternion value9 = new Quaternion(x3, y3, z3, w);
				result.refParams.AddRefParam(name, value9);
			}
			else if (a == "eulerangle")
			{
				float x4 = float.Parse(paramNode.Attribute("x"));
				float y4 = float.Parse(paramNode.Attribute("y"));
				float z4 = float.Parse(paramNode.Attribute("z"));
				Quaternion value10 = Quaternion.Euler(x4, y4, z4);
				result.refParams.AddRefParam(name, value10);
			}
			else if (a == "enum")
			{
				string s = paramNode.Attribute("value");
				int value11 = int.Parse(s);
				result.refParams.AddRefParam(name, value11);
			}
		}

		private void SetEventField(Action action, BaseEvent _trackEvent, SecurityElement _fieldNode)
		{
			string text = _fieldNode.Tag.ToLower();
			Type type = _trackEvent.GetType();
			FieldInfo field = type.GetField(_fieldNode.Attribute("name"));
			if (field == null)
			{
				return;
			}
			bool flag = false;
			string name = string.Empty;
			if (_fieldNode.Attribute("useRefParam") != null)
			{
				flag = bool.Parse(_fieldNode.Attribute("useRefParam"));
			}
			if (flag && _fieldNode.Attribute("refParamName") != null)
			{
				name = _fieldNode.Attribute("refParamName");
			}
			if (text == "float")
			{
				float num = float.Parse(_fieldNode.Attribute("value"));
				field.SetValue(_trackEvent, num);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref num))
					{
						field.SetValue(_trackEvent, num);
					}
				}
			}
			else if (text == "int")
			{
				int num2 = int.Parse(_fieldNode.Attribute("value"));
				field.SetValue(_trackEvent, num2);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref num2))
					{
						field.SetValue(_trackEvent, num2);
					}
				}
			}
			else if (text == "templateobject" || text == "trackobject")
			{
				int num3 = int.Parse(_fieldNode.Attribute("id"));
				field.SetValue(_trackEvent, num3);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref num3))
					{
						field.SetValue(_trackEvent, num3);
					}
				}
			}
			else if (text == "uint")
			{
				uint num4 = uint.Parse(_fieldNode.Attribute("value"));
				field.SetValue(_trackEvent, num4);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref num4))
					{
						field.SetValue(_trackEvent, num4);
					}
				}
			}
			else if (text == "bool")
			{
				bool flag2 = bool.Parse(_fieldNode.Attribute("value"));
				field.SetValue(_trackEvent, flag2);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref flag2))
					{
						field.SetValue(_trackEvent, flag2);
					}
				}
			}
			else if (text == "string")
			{
				string value = _fieldNode.Attribute("value");
				field.SetValue(_trackEvent, value);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					object obj = null;
					if (action.refParams.GetRefParam(name, ref obj))
					{
						field.SetValue(_trackEvent, value);
					}
				}
			}
			else if (text == "vector3")
			{
				float x = float.Parse(_fieldNode.Attribute("x"));
				float y = float.Parse(_fieldNode.Attribute("y"));
				float z = float.Parse(_fieldNode.Attribute("z"));
				Vector3 vector = new Vector3(x, y, z);
				field.SetValue(_trackEvent, vector);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref vector))
					{
						field.SetValue(_trackEvent, vector);
					}
				}
			}
			else if (text == "vector3i")
			{
				int x2 = int.Parse(_fieldNode.Attribute("x"));
				int y2 = int.Parse(_fieldNode.Attribute("y"));
				int z2 = int.Parse(_fieldNode.Attribute("z"));
				VInt3 vInt = new VInt3(x2, y2, z2);
				field.SetValue(_trackEvent, vInt);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref vInt))
					{
						field.SetValue(_trackEvent, vInt);
					}
				}
			}
			else if (text == "quaternion")
			{
				float x3 = float.Parse(_fieldNode.Attribute("x"));
				float y3 = float.Parse(_fieldNode.Attribute("y"));
				float z3 = float.Parse(_fieldNode.Attribute("z"));
				float w = float.Parse(_fieldNode.Attribute("w"));
				Quaternion quaternion = new Quaternion(x3, y3, z3, w);
				field.SetValue(_trackEvent, quaternion);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref quaternion))
					{
						field.SetValue(_trackEvent, quaternion);
					}
				}
			}
			else if (text == "eulerangle")
			{
				float x4 = float.Parse(_fieldNode.Attribute("x"));
				float y4 = float.Parse(_fieldNode.Attribute("y"));
				float z4 = float.Parse(_fieldNode.Attribute("z"));
				Quaternion quaternion2 = Quaternion.Euler(x4, y4, z4);
				field.SetValue(_trackEvent, quaternion2);
				if (flag)
				{
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref quaternion2))
					{
						field.SetValue(_trackEvent, quaternion2);
					}
				}
			}
			else if (text == "enum")
			{
				field.SetValue(_trackEvent, Enum.Parse(field.FieldType, _fieldNode.Attribute("value")));
				if (flag)
				{
					int num5 = 0;
					action.refParams.AddRefData(name, field, _trackEvent);
					if (action.refParams.GetRefParam(name, ref num5))
					{
						field.SetValue(_trackEvent, num5);
					}
				}
			}
			else if (text == "array")
			{
				if (_fieldNode.Children != null)
				{
					string a = _fieldNode.Attribute("type").ToLower();
					if (a == "vector3i")
					{
						VInt3[] array = new VInt3[_fieldNode.Children.Count];
						int num6 = 0;
						foreach (SecurityElement securityElement in _fieldNode.Children)
						{
							int x5 = int.Parse(securityElement.Attribute("x"));
							int y5 = int.Parse(securityElement.Attribute("y"));
							int z5 = int.Parse(securityElement.Attribute("z"));
							VInt3 vInt2 = new VInt3(x5, y5, z5);
							array[num6++] = vInt2;
						}
						field.SetValue(_trackEvent, array);
					}
					else if (a == "int")
					{
						int[] array2 = new int[_fieldNode.Children.Count];
						int num7 = 0;
						foreach (SecurityElement securityElement2 in _fieldNode.Children)
						{
							string s = securityElement2.Attribute("value");
							int num8 = int.Parse(s);
							array2[num7++] = num8;
						}
						field.SetValue(_trackEvent, array2);
					}
					else if (a == "templateobject" || a == "trackobject")
					{
						int[] array3 = new int[_fieldNode.Children.Count];
						int num9 = 0;
						foreach (SecurityElement securityElement3 in _fieldNode.Children)
						{
							string s2 = securityElement3.Attribute("id");
							int num10 = int.Parse(s2);
							array3[num9++] = num10;
						}
						field.SetValue(_trackEvent, array3);
					}
					else if (a == "uint")
					{
						uint[] array4 = new uint[_fieldNode.Children.Count];
						int num11 = 0;
						foreach (SecurityElement securityElement4 in _fieldNode.Children)
						{
							string s3 = securityElement4.Attribute("value");
							uint num12 = uint.Parse(s3);
							array4[num11++] = num12;
						}
						field.SetValue(_trackEvent, array4);
					}
					else if (a == "bool")
					{
						bool[] array5 = new bool[_fieldNode.Children.Count];
						int num13 = 0;
						foreach (SecurityElement securityElement5 in _fieldNode.Children)
						{
							string value2 = securityElement5.Attribute("value");
							bool flag3 = bool.Parse(value2);
							array5[num13++] = flag3;
						}
						field.SetValue(_trackEvent, array5);
					}
					else if (a == "float")
					{
						float[] array6 = new float[_fieldNode.Children.Count];
						int num14 = 0;
						foreach (SecurityElement securityElement6 in _fieldNode.Children)
						{
							string s4 = securityElement6.Attribute("value");
							float num15 = float.Parse(s4);
							array6[num14++] = num15;
						}
						field.SetValue(_trackEvent, array6);
					}
					else if (a == "string")
					{
						string[] array7 = new string[_fieldNode.Children.Count];
						int num16 = 0;
						foreach (SecurityElement securityElement7 in _fieldNode.Children)
						{
							string text2 = securityElement7.Attribute("value");
							array7[num16++] = text2;
						}
						field.SetValue(_trackEvent, array7);
					}
				}
			}
			else
			{
				Debug.LogError("Invalid field type \"" + text + "\"!");
			}
		}

		public bool GetActionTemplateObjectsAndPredefRefParams(string actionName, ref List<TemplateObject> objs, ref List<string> refnames)
		{
			if (actionName == null)
			{
				return false;
			}
			ActionCommonData actionCommonData;
			if (this.actionCommonDataSet.ContainsKey(actionName))
			{
				actionCommonData = this.actionCommonDataSet[actionName];
			}
			else
			{
				CBinaryObject cBinaryObject = ActionManager.Instance.resLoader.LoadAge(actionName) as CBinaryObject;
				if (cBinaryObject == null)
				{
					return false;
				}
				actionCommonData = new ActionCommonData();
				SecurityParser securityParser = new SecurityParser();
				securityParser.LoadXml(Encoding.UTF8.GetString(cBinaryObject.m_data));
				Singleton<CResourceManager>.GetInstance().RemoveCachedResource(actionName);
				SecurityElement securityElement = securityParser.SelectSingleNode("Project");
				SecurityElement securityElement2 = (securityElement == null) ? null : securityElement.SearchForChildByTag("TemplateObjectList");
				if (securityElement2 != null)
				{
					foreach (SecurityElement securityElement3 in securityElement2.Children)
					{
						if (!(securityElement3.Tag != "TemplateObject"))
						{
							TemplateObject templateObject = new TemplateObject();
							templateObject.name = securityElement3.Attribute("objectName");
							templateObject.id = int.Parse(securityElement3.Attribute("id"));
							templateObject.isTemp = bool.Parse(securityElement3.Attribute("isTemp"));
							actionCommonData.templateObjects.Add(templateObject);
						}
					}
				}
				SecurityElement securityElement4 = (securityElement == null) ? null : securityElement.SearchForChildByTag("RefParamList");
				if (securityElement4 != null)
				{
					foreach (SecurityElement securityElement5 in securityElement4.Children)
					{
						string text = securityElement5.Attribute("name");
						if (text.StartsWith("_"))
						{
							actionCommonData.predefRefParamNames.Add(text);
						}
					}
				}
				this.actionCommonDataSet.Add(actionName, actionCommonData);
			}
			if (actionCommonData != null)
			{
				objs.Clear();
				refnames.Clear();
				for (int i = 0; i < actionCommonData.templateObjects.Count; i++)
				{
					objs.Add(actionCommonData.templateObjects[i]);
				}
				for (int j = 0; j < actionCommonData.predefRefParamNames.Count; j++)
				{
					refnames.Add(actionCommonData.predefRefParamNames[j]);
				}
			}
			return true;
		}

		public ActionSet FilterActionsByGameObject(GameObject obj, string nameInAction)
		{
			ActionSet actionSet = new ActionSet();
			for (int i = 0; i < this.actionList.Count; i++)
			{
				Action action = this.actionList[i];
				int index = 0;
				action.templateObjectIds = this.LoadTemplateObjectList(action);
				bool flag = action.TemplateObjectIds.TryGetValue(nameInAction, out index);
				if (flag && action.GetGameObject(index) == obj)
				{
					actionSet.actionSet.Add(action, true);
				}
			}
			return actionSet;
		}

		public void SetResLoader(AssetLoader loader)
		{
			this.resLoader = loader;
		}
	}
}
