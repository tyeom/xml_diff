using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xml_diff.Common.EventAggregator;
using XmlDiffLib;

namespace xml_diff.Messaging
{
    public class ChangeModeMessaging : IApplicationEvent
    {
        public ChangeModeMessaging(XmlDiffControl.EModeType modeType)
        {
            ModeType = modeType;
        }

        public XmlDiffControl.EModeType ModeType
        {
            get;
            private set;
        }
    }
}
