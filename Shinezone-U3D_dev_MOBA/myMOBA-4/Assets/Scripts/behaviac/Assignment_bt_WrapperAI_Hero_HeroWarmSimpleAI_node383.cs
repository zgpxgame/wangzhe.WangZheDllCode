using System;

namespace behaviac
{
	internal class Assignment_bt_WrapperAI_Hero_HeroWarmSimpleAI_node383 : Assignment
	{
		protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
		{
			EBTStatus result = EBTStatus.BT_SUCCESS;
			int value = 13000;
			pAgent.SetVariable<int>("p_escapeRange", value, 750283102u);
			return result;
		}
	}
}
