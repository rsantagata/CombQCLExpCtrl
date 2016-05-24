var expParams;
var channelNames;
var x_axis_name;
var nameIndexLookup;


function initialisePlot(domElement, expParams) {
    x_axis_name = expParams.AINames[0]; //Convention: first channel is always the x_axis.
    channelNames = expParams.AINames.slice(1, expParams.AINames.length + 1); //The others are the channels to plot.

    nameIndexLookup = createLookupTable(channelNames);
    var initData = channelNames.map(function(d) {
        return { name: d, x: [], y: [] };
    })

    //Create empty plot
    Plotly.newPlot(domElement, initData, { margin: { t: 50, b: 50, l: 50, r: 50 } }, { modeBarButtonsToRemove: ['sendDataToCloud'], showLink: false, displaylogo: false });

    this.expParams = expParams;
};

//Convention: the incoming data must be of the form {x_val: X, y1_val: Y1, y2_val: Y2  ...}.
function appendData(domElement, newXYPairs) {
    for (var i = 0; i < newXYPairs.length; i++) {
        channelNames.map(function(d) {
            domElement.data[nameIndexLookup[d]].x.push(newXYPairs[i][x_axis_name]);
            domElement.data[nameIndexLookup[d]].y.push(newXYPairs[i][d]);
        });
    }
    Plotly.redraw(domElement);
};

function deleteData(domElement) {
    channelNames.map(function(d) {
        domElement.data[nameIndexLookup[d]].x = [];
        domElement.data[nameIndexLookup[d]].y = [];
    });

    Plotly.redraw(domElement);
};

function createLookupTable(names) {
    var rv = {};
    for (var i = 0; i < names.length; i++)
        rv[names[i]] =  i;
    return rv;
}
