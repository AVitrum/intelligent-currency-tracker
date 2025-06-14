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
        return {w, h, cw, ch, dr, ps, min};
    }

    function drawAxes() {
        const d = getDims();
        ctx.beginPath();
        ctx.moveTo(padding, padding);
        ctx.lineTo(padding, d.h - padding);
        ctx.lineTo(d.w - padding, d.h - padding);
        ctx.strokeStyle = '#1D3557';
        ctx.lineWidth = 2;
        ctx.stroke();
        ctx.font = '16px Arial';
        ctx.fillStyle = '#1D3557';
        ctx.fillText('Date', d.w / 2 - 20, d.h - 10);
        ctx.save();
        ctx.translate(15, d.h / 2 + 20);
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
        ctx.fillStyle = '#FFFFFF';
        ctx.fillRect(0, 0, d.w, d.h);

        drawAxes();

        const candleWidth = Math.max(4, d.ps * 0.6);

        for (let i = 0; i < values.length; i++) {
            const x = padding + d.ps * (i + 0.5);
            const y = yForValue(values[i], d);
            ctx.beginPath();
            ctx.rect(
                x - candleWidth / 2,
                y,
                candleWidth,
                d.h - padding - y
            );
            ctx.fillStyle = '#2B6CB0';
            ctx.fill();

            if (i > 0) {
                ctx.beginPath();
                ctx.moveTo(x - candleWidth / 2, y);
                ctx.lineTo(x + candleWidth / 2, y);
                ctx.strokeStyle = values[i] > values[i - 1] ? '#4CAF50' : '#D32F2F';
                ctx.lineWidth = 3;
                ctx.stroke();
            }
        }
    }

    function showTooltip(i) {
        const d = getDims();
        const x = padding + d.ps * (i + 0.5);
        const y = yForValue(values[i], d);
        const r = canvas.getBoundingClientRect();
        tooltipEl.innerHTML = `Date: ${dates[i] || ''}<br>Value: ${values[i]}`;
        tooltipEl.style.left = `${r.left + x}px`;
        tooltipEl.style.top = `${r.top + y}px`;
        tooltipEl.style.display = 'block';
    }

    function hideTooltip() {
        tooltipEl.style.display = 'none';
    }

    function onMouseMove(e) {
        const d = getDims();
        const r = canvas.getBoundingClientRect();
        const mx = e.clientX - r.left;
        const candleWidth = Math.max(4, d.ps * 0.6);
        let found = null;
        for (let i = 0; i < values.length; i++) {
            const x = padding + d.ps * (i + 0.5);
            if (Math.abs(mx - x) < candleWidth / 2) {
                found = i;
                break;
            }
        }
        drawCandles();
        if (found !== null) showTooltip(found);
        else hideTooltip();
    }

    resizeCanvas();
    drawCandles();

    canvas.addEventListener('mousemove', onMouseMove);
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