$(document).ready(function () {
    loadGraphs(); // Тепер завантажуємо обидва графіки
});

function loadGraphs() {
    $.ajax({
        url: '/home/creategraphdata/',
        method: 'GET',
        success: function(response) {
            if(response.success) {
                createIncomeGraph(response.data.incomeData);
                createExpenseGraph(response.data.expenseData);
            } else {
                console.error("Failed to load data for the graphs.");
            }
        },
        error: function(error) {
            console.error("Error occurred while loading the graph data:", error);
        }
    });
}

function createIncomeGraph(data) {
    const colors = generateColors(data.length);
    new Chart(document.getElementById('incomeChart').getContext('2d'), {
        type: 'pie',
        data: {
            labels: data.map(item => item.category),
            datasets: [{
                label: "Прибутки:",
                data: data.map(item => item.amount),
                backgroundColor: colors,
                hoverOffset: 4
            }]
        }
    });
}

function createExpenseGraph(data) {
    const colors = generateColors(data.length);
    new Chart(document.getElementById('expenseChart').getContext('2d'), {
        type: 'pie',
        data: {
            labels: data.map(item => item.category),
            datasets: [{
                label: "Витрати:",
                data: data.map(item => item.amount),
                backgroundColor: colors,
                hoverOffset: 4
            }]
        }
    });
}

function generateColors(length) {
    const colors = [];
    for (let i = 0; i < length; i++) {
        colors.push(`hsl(${i * 360 / length}, 100%, 50%)`);
    }
    return colors;
}
