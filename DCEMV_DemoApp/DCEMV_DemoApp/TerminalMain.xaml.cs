using Common;
using EMVProtocol;
using FormattingUtils;
using ISO7816Protocol;
using SPDHProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPIPDriver;
using TLVProtocol;
using Xamarin.Forms;

namespace XTerminal
{
    public partial class TerminalMain : TabbedPage
    {
        public static Logger Logger = new Logger(typeof(EMVMain));

        public TerminalMain(ICardInterfaceManger cardInterfaceManger, IOnlineApprover onlineApprover, IConfigurationProvider configProvider, TCPClientStream tcpClientStream)
        {
            InitializeComponent();
            
            Children.Add(new EMVMain(cardInterfaceManger, onlineApprover, configProvider, tcpClientStream));
            Children.Add(new DesFireMain(cardInterfaceManger));
        }
    }
}
