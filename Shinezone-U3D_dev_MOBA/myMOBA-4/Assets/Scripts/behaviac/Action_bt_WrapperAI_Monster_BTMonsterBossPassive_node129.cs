using Assets.Scripts.GameLogic;
using System;

namespace behaviac
{
	internal class Action_bt_WrapperAI_Monster_BTMonsterBossPassive_node129 : Action
	{
		private int method_p0;

		public Action_bt_WrapperAI_Monster_BTMonsterBossPassive_node129()
		{
			this.method_p0 = 5000;
		}

		protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
		{
			((ObjAgent)pAgent).NotifySelfCampSelfWillAttack(this.method_p0);
			return EBTStatus.BT_SUCCESS;
		}
	}
}
