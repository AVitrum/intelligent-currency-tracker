function drawChart(data, dates) {
    const canvas = document.getElementById('currencyChart');
    const container = canvas.parentElement;

    let canvasWidth = container.clientWidth;
    let canvasHeight = container.clientHeight;
    canvas.width = canvasWidth;
    canvas.height = canvasHeight;

    if (!canvas.getContext) return;
    const ctx = canvas.getContext('2d');

    const padding = 50;

    function getChartDimensions() {
        const chartWidth = canvas.width - padding * 2;
        const chartHeight = canvas.height - padding * 2;

        const max = Math.max(...data);
        const min = Math.min(...data);
        const dataRange = max - min;
        const pointSpacing = chartWidth / (data.length - 1);

        return { chartWidth, chartHeight, dataRange, pointSpacing, min, max };
    }

    function setBackground() {
        ctx.fillStyle = '#FFFAEC';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    }

    function drawAxes() {
        const { chartHeight } = getChartDimensions();
        ctx.beginPath();
        ctx.moveTo(padding, padding);
        ctx.lineTo(padding, canvas.height - padding);
        ctx.lineTo(canvas.width - padding, canvas.height - padding);
        ctx.strokeStyle = '#3D3D3D';
        ctx.lineWidth = 2;
        ctx.stroke();

        ctx.font = "16px Arial";
        ctx.fillStyle = '#3D3D3D';
        ctx.fillText("Date", canvas.width / 2 - 20, canvas.height - 10);

        ctx.save();
        ctx.translate(15, canvas.height / 2 + 20);
        ctx.rotate(-Math.PI / 2);
        ctx.fillText("Value", 0, 0);
        ctx.restore();
    }

    let pathPoints = [];
    function updatePathPoints() {
        const { chartWidth, chartHeight, dataRange, min, pointSpacing } = getChartDimensions();
        pathPoints = [];
        for (let i = 0; i < data.length; i++) {
            const x = padding + i * pointSpacing;
            const scaleValue = dataRange === 0 ? 0 : (data[i] - min) / dataRange;
            const y = canvas.height - padding - (scaleValue * chartHeight);
            pathPoints.push({ x, y, value: data[i], date: dates[i] });
        }
    }

    function drawChartElements() {
        setBackground();
        drawAxes();
        const { chartWidth, chartHeight } = getChartDimensions();

        ctx.beginPath();
        ctx.moveTo(padding, canvas.height - padding);
        for (let i = 0; i < data.length; i++) {
            const point = pathPoints[i];
            ctx.lineTo(point.x, point.y);
        }
        ctx.lineTo(pathPoints[pathPoints.length - 1].x, canvas.height - padding);
        ctx.closePath();
        ctx.fillStyle = 'rgba(87, 142, 126, 0.2)';
        ctx.fill();

        ctx.beginPath();
        for (let i = 0; i < data.length; i++) {
            const point = pathPoints[i];
            if (i === 0) {
                ctx.moveTo(point.x, point.y);
            } else {
                ctx.lineTo(point.x, point.y);
            }
        }
        ctx.strokeStyle = '#578E7E';
        ctx.lineWidth = 2;
        ctx.stroke();

        for (let i = 0; i < data.length; i++) {
            const point = pathPoints[i];
            ctx.beginPath();
            ctx.arc(point.x, point.y, 4, 0, Math.PI * 2);
            ctx.fillStyle = '#578E7E';
            ctx.fill();
        }
    }

    let tooltip = null;
    function showTooltip(x, y, value, date) {
        tooltip = { x, y, value, date };
        drawChartElements();
        drawTooltip();
    }

    function drawTooltip() {
        if (!tooltip) return;

        const paddingX = 10;
        const paddingY = 10;

        ctx.font = '12px Arial';
        const dateText = `Date: ${tooltip.date}`;
        const valueText = `Value: ${tooltip.value.toFixed(2)}`;
        const tooltipWidth = Math.max(ctx.measureText(dateText).width, ctx.measureText(valueText).width) + paddingX * 2;
        const tooltipHeight = 30 + paddingY * 2;

        let tooltipX = tooltip.x + paddingX;
        let tooltipY = tooltip.y - tooltipHeight;

        if (tooltipX + tooltipWidth > canvas.width) {
            tooltipX = canvas.width - tooltipWidth - paddingX;
        }

        if (tooltipY + tooltipHeight > canvas.height) {
            tooltipY = canvas.height - tooltipHeight - paddingY;
        }

        if (tooltipY < paddingY) {
            tooltipY = tooltip.y + paddingY;
        }

        const radius = 8;
        ctx.fillStyle = '#3D3D3D';
        ctx.beginPath();
        ctx.moveTo(tooltipX + radius, tooltipY);
        ctx.lineTo(tooltipX + tooltipWidth - radius, tooltipY);
        ctx.arcTo(tooltipX + tooltipWidth, tooltipY, tooltipX + tooltipWidth, tooltipY + tooltipHeight, radius);
        ctx.lineTo(tooltipX + tooltipWidth, tooltipY + tooltipHeight - radius);
        ctx.arcTo(tooltipX + tooltipWidth, tooltipY + tooltipHeight, tooltipX + tooltipWidth - radius, tooltipY + tooltipHeight, radius);
        ctx.lineTo(tooltipX + radius, tooltipY + tooltipHeight);
        ctx.arcTo(tooltipX, tooltipY + tooltipHeight, tooltipX, tooltipY + tooltipHeight - radius, radius);
        ctx.lineTo(tooltipX, tooltipY + radius);
        ctx.arcTo(tooltipX, tooltipY, tooltipX + radius, tooltipY, radius);
        ctx.closePath();
        ctx.fill();

        const dateTextWidth = ctx.measureText(dateText).width;
        const valueTextWidth = ctx.measureText(valueText).width;
        const textX = tooltipX + (tooltipWidth - Math.max(dateTextWidth, valueTextWidth)) / 2;
        const textY = tooltipY + (tooltipHeight / 2) - paddingY;

        ctx.fillStyle = '#FFF';
        ctx.fillText(dateText, textX, textY);
        ctx.fillText(valueText, textX, textY + 15);
    }

    function isMouseOverPoint(x, y) {
        const mouseRadius = 10;
        for (let i = 0; i < pathPoints.length; i++) {
            const point = pathPoints[i];
            const distance = Math.sqrt(Math.pow(point.x - x, 2) + Math.pow(point.y - y, 2));
            if (distance <= mouseRadius) {
                showTooltip(point.x, point.y, point.value, point.date);
                return;
            }
        }
        tooltip = null;
        drawChartElements();
    }

    canvas.addEventListener('mousemove', function (event) {
        const rect = canvas.getBoundingClientRect();
        const mouseX = event.clientX - rect.left;
        const mouseY = event.clientY - rect.top;
        isMouseOverPoint(mouseX, mouseY);
    });

    window.addEventListener('resize', function () {
        const container = canvas.parentElement;
        let canvasWidth = container.clientWidth;
        let canvasHeight = container.clientHeight;
        canvas.width = canvasWidth;
        canvas.height = canvasHeight;

        updatePathPoints();
        drawChartElements();
    });

    updatePathPoints();
    drawChartElements();
}
