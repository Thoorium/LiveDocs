export function initialize(elementId) {
    const graphdiv = document.querySelector('#' + elementId);
    var graphdata = JSON.parse(graphdiv.dataset.mxgraph);
    var graph = new mxGraph(graphdiv);

    graph.resetViewOnRootChange = false;
    graph.foldingEnabled = false;

    // NOTE: Tooltips require CSS
    graph.setTooltips(false);
    graph.setEnabled(false);

    fetch(graphdata.url)
        .then(res => res.text())
        .then(text => {
            const parser = new DOMParser();
            var xml = parser.parseFromString(text, "application/xml");
            var diagram = xml.getElementsByTagName("diagram");
            var content = mxUtils.getTextContent(diagram[0]);
            xml = Graph.decompress(content);

            // Removes all illegal control characters before parsing
            var checked = [];

            for (var i = 0; i < xml.length; i++) {
                var code = xml.charCodeAt(i);

                // Removes all control chars except TAB, LF and CR
                if (code >= 32 || code == 9 || code == 10 || code == 13) {
                    checked.push(xml.charAt(i));
                }
            }

            xml = checked.join('');
            var xmlDoc = mxUtils.parseXml(xml);
            var codec = new mxCodec(xmlDoc);
            codec.decode(codec.document.documentElement, graph.getModel());
        });

    graph.maxFitScale = 1;
    graph.fit();
    graph.center(true, false);
}