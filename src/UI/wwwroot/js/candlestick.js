const candlestickColors = {
    primary: '#6366F1',
    primaryLight: '#818CF8',
    success: '#22C55E',
    successLight: '#4ADE80',
    error: '#EF4444',
    errorLight: '#F87171',
    background: '#FFFFFF',
    gridLine: '#E2E8F0',
    text: '#0F172A',
    textSecondary: '#64748B'
};

function drawCandlestickChart(data, dates) {
    const canvas = document.getElementById('currencyChart');
    const tooltipEl = document.getElementById('chartTooltip');
    if (!canvas || !tooltipEl) return;
    const ctx = canvas.getContext('2d');
    const padding = 50;

    let values = Array.isArray(data) && typeof data[0] === 'object' && 'close' in data[0]
        ? data.map(x => x.close)
        : data;

    function resizeCanvas() {
        const r = canvas.getBoundingClientRect();
        canvas.width = r.width * window.devicePixelRatio;
        canvas.height = r.height * window.devicePixelRatio;
        ctx.setTransform(window.devicePixelRatio, 0, 0, window.devicePixelRatio, 0, 0);
    }

    function getDims() {
        const r = canvas.getBoundingClientRect();
        const w = r.width;
        const h = r.height;
        const cw = w - padding * 2;
        const ch = h - padding * 2;
        const max = Math.max(...values);
        const min = Math.min(...values);
        const dr = max - min || 1;
        const ps = values.length > 0 ? cw / values.length : 0;
        return {w, h, cw, ch, dr, ps, min, max};
    }

    function drawAxes() {
        const d = getDims();
        
        // Draw grid lines
        ctx.strokeStyle = candlestickColors.gridLine;
        ctx.lineWidth = 1;
        
        const numGridLines = 5;
        for (let i = 0; i <= numGridLines; i++) {
            const y = padding + (d.ch / numGridLines) * i;
            ctx.beginPath();
            ctx.setLineDash([5, 5]);
            ctx.moveTo(padding, y);
            ctx.lineTo(d.w - padding, y);
            ctx.stroke();
        }
        ctx.setLineDash([]);
        
        // Axes
        ctx.beginPath();
        ctx.moveTo(padding, padding);
        ctx.lineTo(padding, d.h - padding);
        ctx.lineTo(d.w - padding, d.h - padding);
        ctx.strokeStyle = candlestickColors.gridLine;
        ctx.lineWidth = 2;
        ctx.stroke();
        
        // Labels
        ctx.font = '600 14px Inter, sans-serif';
        ctx.fillStyle = candlestickColors.textSecondary;
        ctx.fillText('Date', d.w / 2 - 15, d.h - 12);
        ctx.save();
        ctx.translate(18, d.h / 2 + 20);
        ctx.rotate(-Math.PI / 2);
        ctx.fillText('Value', 0, 0);
        ctx.restore();
    }

    function yForValue(v, d) {
        return d.h - padding - ((v - d.min) / d.dr) * d.ch;
    }

    function drawCandles() {
        const d = getDims();
        ctx.clearRect(0, 0, d.w, d.h);
        ctx.fillStyle = candlestickColors.background;
        ctx.fillRect(0, 0, d.w, d.h);

        drawAxes();

        const candleWidth = Math.max(6, d.ps * 0.65);
        const candleRadius = Math.min(3, candleWidth / 4);

        for (let i = 0; i < values.length; i++) {
            const x = padding + d.ps * (i + 0.5);
            const y = yForValue(values[i], d);
            const barHeight = d.h - padding - y;
            
            // Determine color based on trend
            const isPositive = i === 0 || values[i] >= values[i - 1];
            const barColor = isPositive ? candlestickColors.success : candlestickColors.error;
            const barColorLight = isPositive ? candlestickColors.successLight : candlestickColors.errorLight;
            
            // Create gradient for bar
            const gradient = ctx.createLinearGradient(x - candleWidth / 2, 0, x + candleWidth / 2, 0);
            gradient.addColorStop(0, barColor);
            gradient.addColorStop(0.5, barColorLight);
            gradient.addColorStop(1, barColor);
            
            // Draw rounded bar
            ctx.beginPath();
            ctx.roundRect(
                x - candleWidth / 2,
                y,
                candleWidth,
                barHeight,
                [candleRadius, candleRadius, 0, 0]
            );
            ctx.fillStyle = gradient;
            ctx.fill();
            
            // Add subtle shadow
            ctx.shadowColor = 'rgba(0, 0, 0, 0.1)';
            ctx.shadowBlur = 4;
            ctx.shadowOffsetX = 2;
            ctx.shadowOffsetY = 2;
            ctx.fill();
            ctx.shadowColor = 'transparent';
        }
    }

    function showTooltip(i) {
        const d = getDims();
        const x = padding + d.ps * (i + 0.5);
        const y = yForValue(values[i], d);
        const r = canvas.getBoundingClientRect();
        const isPositive = i === 0 || values[i] >= values[i - 1];
        const changeColor = isPositive ? candlestickColors.success : candlestickColors.error;
        const changeIcon = isPositive ? '↑' : '↓';
        
        let changePercent = '';
        if (i > 0) {
            const change = ((values[i] - values[i - 1]) / values[i - 1] * 100).toFixed(2);
            changePercent = `<br><span style="color: ${changeColor}">${changeIcon} ${change}%</span>`;
        }
        
        tooltipEl.innerHTML = `<strong>${dates[i] || ''}</strong><br>Value: <span style="color: ${candlestickColors.primary}">${values[i].toFixed(4)}</span>${changePercent}`;
        tooltipEl.style.left = `${r.left + x}px`;
        tooltipEl.style.top = `${r.top + y - 10}px`;
        tooltipEl.style.display = 'block';
    }

    function hideTooltip() {
        tooltipEl.style.display = 'none';
    }

    function onMouseMove(e) {
        const d = getDims();
        const r = canvas.getBoundingClientRect();
        const mx = e.clientX - r.left;
        const candleWidth = Math.max(6, d.ps * 0.65);
        let found = null;
        
        for (let i = 0; i < values.length; i++) {
            const x = padding + d.ps * (i + 0.5);
            if (Math.abs(mx - x) < candleWidth) {
                found = i;
                break;
            }
        }
        
        drawCandles();
        
        if (found !== null) {
            // Highlight the bar
            const x = padding + d.ps * (found + 0.5);
            const y = yForValue(values[found], d);
            const barHeight = d.h - padding - y;
            
            ctx.beginPath();
            ctx.roundRect(
                x - candleWidth / 2 - 2,
                y - 2,
                candleWidth + 4,
                barHeight + 4,
                [4, 4, 0, 0]
            );
            ctx.strokeStyle = candlestickColors.primary;
            ctx.lineWidth = 2;
            ctx.stroke();
            
            showTooltip(found);
        } else {
            hideTooltip();
        }
    }

    resizeCanvas();
    drawCandles();

    canvas.addEventListener('mousemove', onMouseMove);
    canvas.addEventListener('mouseleave', () => {
        drawCandles();
        hideTooltip();
    });
    window.addEventListener('resize', () => {
        resizeCanvas();
        drawCandles();
        hideTooltip();
    });
}

window.downloadTableAsPdf = function (tableId) {
    if (typeof window.jspdf === "undefined" && typeof window.jsPDF === "undefined") {
        alert("jsPDF library is not loaded.");
        return;
    }
    if (typeof window.html2canvas === "undefined") {
        alert("html2canvas library is not loaded.");
        return;
    }
    const table = document.getElementById(tableId);
    if (!table) return;
    window.html2canvas(table).then(function (canvas) {
        const imgData = canvas.toDataURL("image/png");
        const pdf = new window.jspdf.jsPDF({
            orientation: "landscape",
            unit: "pt",
            format: [canvas.width, canvas.height]
        });
        pdf.addImage(imgData, "PNG", 0, 0, canvas.width, canvas.height);
        pdf.save("table.pdf");
    });
};