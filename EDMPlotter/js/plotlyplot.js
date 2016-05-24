var expParams;
var showTraceBools;

function initialisePlot(domElement, expParams) {
    Plotly.newPlot(domElement, [{ x: [0] }], { margin: { t: 50, b: 50, l: 50, r: 50 } }, { modeBarButtonsToRemove: ['sendDataToCloud'], showLink: false, displaylogo: false });
    Plotly.deleteTraces(domElement, 0);
    this.expParams = expParams;
};

function addData(domElement, data) {
    var plotDataArray = [];
    for (var j = 0; j < expParams.AINames.length; j++) {
        var tempArray = [];
        for (var i = 0; i < data.length; i++) {
            tempArray.push(data[i][expParams.AINames[j]]);
        }
        plotDataArray[j] = tempArray;
    }

//Convention: the first element is always the x-axis. mapping the plot format on the others. Note the weird index assignment for slice.
    var final_data = plotDataArray.slice(1, plotDataArray.length + 1).map(function(d) {
        return { x: plotDataArray[0], y: d, visible: 'true' }
    });

//Making sure the visiblility toggle is carried through from previous scan.
    for(var i = 0; i<final_data.length; i++) {
        final_data[i].visible = (showTraceBools == undefined) ? true : showTraceBools[i];
    }
    
    final_data.map(function(d) {
        Plotly.addTraces(document.getElementById("plot"), d);
    });

};

function appendData(domElement, newXYPairs, AIIndex) {
    for (var i = 0; i < newXYPairs.length; i++) {
        domElement.data[AIIndex].x.push(newXYPairs[i][0]);
        domElement.data[AIIndex].y.push(newXYPairs[i][1]);
    }
    Plotly.redraw(domElement);
};

function deleteData(domElement) {
    this.showTraceBools = domElement.data.map(function(d) {return d.visible});
    var numberOfTracesToDelete = expParams.AINames.length - 1;
    var traceIndicesToDelete = []; //prepping for update
    //Note the convention that the first 'name' is always the x-axis. Not a trace!
    for (var j = -(numberOfTracesToDelete); j < 0; j++) { //quirky indices for the deletion to happen in right order
        traceIndicesToDelete.push(j);
    }
    Plotly.deleteTraces(domElement, traceIndicesToDelete);

};
