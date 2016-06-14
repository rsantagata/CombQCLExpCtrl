var channelNames;
var x_axis_name;
var nameIndexLookup;


function initialisePlot(domElement, x_name, input_names) {
    x_axis_name = x_name;
    channelNames = input_names; //The others are the channels to plot.

    nameIndexLookup = createLookupTable(channelNames); //To lake it easier to append data to a particular channel later.
    
    //Start with empty dataset.
    var initData = channelNames.map(function(d) {
        return { name: d, x: [], y: [] };
    })

    //Create empty plot
    Plotly.newPlot(domElement, initData, {
        margin: { t: 50, b: 50, l: 50, r: 50 },
        xaxis: {
            title: x_axis_name,
            titlefont: {
                family: 'Courier New, monospace',
                size: 12,
                color: '#7f7f7f'
            }
        }
    }, {
        modeBarButtonsToRemove: ['sendDataToCloud', 'toImage'],
        showLink: false,
        displaylogo: false,
        scrollZoom: true,
        hovermode: 'closest',

    });
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

function updateXAxisLabel(domElement, newXAxisLabel) {
	var update = {
        margin: { t: 50, b: 50, l: 50, r: 50 },
        xaxis: {
            title: newXAxisLabel,
            titlefont: {
                family: 'Courier New, monospace',
                size: 12,
                color: '#7f7f7f'
            }
        }
    };
	Plotly.relayout(domElement, update)
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
        rv[names[i]] = i;
    return rv;
}
