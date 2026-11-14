from pathlib import Path
from copy import deepcopy
import re
from lxml import etree as ET

root = Path(__file__).resolve().parents[1]
project = root / 'CX Audio'
x = 'http://schemas.microsoft.com/winfx/2006/xaml'
p = 'http://schemas.microsoft.com/winfx/2006/xaml/presentation'
path = project / 'MainWindow.xaml'
tree = ET.parse(str(path))
window = tree.getroot()
cs_path = project / 'MainWindow.xaml.cs'
cs = cs_path.read_text(encoding='utf-8-sig')
views = project / 'Views'
views.mkdir(exist_ok=True)

def named(name):
    return window.xpath('//*[@x:Name=$name]', namespaces={'x': x}, name=name)[0]

aliases = []
handler_names = set()
for view, field, panel_names in [
    ('AudioView', '_audioView', ['AudioPanel']),
    ('ConnectionsView', '_connectionsView', ['SharePanel', 'NetworkPanel', 'ReceiversPanel', 'AndroidPanel']),
    ('SystemView', '_systemView', ['SystemPanel']),
]:
    uc = ET.Element('{'+p+'}UserControl', nsmap={None:p, 'x':x, 'vm':'using:CXAudioOutput.ViewModels'})
    uc.set('{'+x+'}Class', 'CXAudioOutput.Views.'+view)
    stack = ET.SubElement(uc, '{'+p+'}StackPanel', Spacing='16')
    methods = []
    events = set()
    for panel_name in panel_names:
        panel = deepcopy(named(panel_name))
        panel.attrib.pop('Grid.Column', None)
        panel.attrib.pop('Grid.Row', None)
        stack.append(panel)
        for node in panel.iter():
            name = node.get('{'+x+'}Name')
            if name:
                node.set('{'+x+'}FieldModifier', 'internal')
                tag = ET.QName(node).localname
                aliases.append('    private '+tag+' '+name+' => '+field+'.'+name+';')
            for event in ('Click','SelectionChanged','ValueChanged','Toggled','SizeChanged'):
                handler = node.get(event)
                if handler:
                    events.add(handler)
    for handler in sorted(events):
        match = re.search(r'(?:private|internal)\s+(?:async\s+)?void\s+'+re.escape(handler)+r'\(([^)]*)\)', cs)
        if not match:
            raise RuntimeError('Missing event signature '+handler)
        methods.append('    private void '+handler+'('+match.group(1)+') => Host?.'+handler+'(sender, e);')
        handler_names.add(handler)
    ET.indent(uc, space='    ')
    (views/(view+'.xaml')).write_text(ET.tostring(uc, encoding='unicode')+'\n', encoding='utf-8')
    code = '''using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace CXAudioOutput.Views;

public sealed partial class VIEW : UserControl
{
    internal MainWindow? Host { get; set; }

    public VIEW()
    {
        InitializeComponent();
    }

METHODS
}
'''.replace('VIEW',view).replace('METHODS','\n'.join(methods))
    (views/(view+'.xaml.cs')).write_text(code, encoding='utf-8')

old = named('OriginalContent')
old.getparent().remove(old)
# The shell contains only title bar, navigation and frame.
ET.indent(window, space='    ')
path.write_text(ET.tostring(window, encoding='unicode')+'\n', encoding='utf-8')
for handler in handler_names:
    cs = re.sub(r'\bprivate(\s+(?:async\s+)?void\s+'+re.escape(handler)+r'\()', r'internal\1', cs)
cs = cs.replace('        Title = "CX Audio Output";', '        InitializeOwnedViews();\n        Title = "CX Audio Output";', 1)
cs_path.write_text(cs, encoding='utf-8')

fields = '''using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using CXAudioOutput.Views;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private AudioView _audioView = null!;
    private ConnectionsView _connectionsView = null!;
    private SystemView _systemView = null!;

    private void InitializeOwnedViews()
    {
        // Each view owns its XAML namescope and compiled bindings permanently.
        // Pages attach a detached view once, then remain in the Frame cache.
        _audioView = new AudioView { Host = this };
        _connectionsView = new ConnectionsView { Host = this };
        _systemView = new SystemView { Host = this };
    }

ALIASES
}
'''.replace('ALIASES','\n'.join(aliases))
(project/'MainWindow.Views.cs').write_text(fields,encoding='utf-8')
nav_path = project/'MainWindow.Navigation.cs'
nav = nav_path.read_text(encoding='utf-8-sig')
nav = nav.replace('AudioContent => AudioPanel','AudioContent => _audioView')
nav = nav.replace('ShareContent => SharePanel','ConnectionsContent => _connectionsView')
nav = re.sub(r'^    internal FrameworkElement (NetworkContent|ReceiversContent|AndroidContent) => .*?;\n','', nav, flags=re.M)
nav = nav.replace('SystemContent => SystemPanel','SystemContent => _systemView')
nav_path.write_text(nav,encoding='utf-8')
page_path = project/'Pages.cs'
pages = page_path.read_text(encoding='utf-8-sig')
pages = pages.replace('host.ShareContent, host.NetworkContent,\n            host.ReceiversContent, host.AndroidContent','host.ConnectionsContent')
page_path.write_text(pages,encoding='utf-8')
print(f'Migrated 6 original sections into 3 independently owned XAML views; preserved {len(handler_names)} handlers and {len(aliases)} named controls.')
