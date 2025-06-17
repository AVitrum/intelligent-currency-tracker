function drawChart(data, dates) {
    const canvas = document.getElementById('currencyChart');
    const tooltipEl = document.getElementById('chartTooltip');
    const ctx = canvas.getContext('2d');
    const padding = 50;
    let pathPoints = [];

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
        const max = Math.max(...data);
        const min = Math.min(...data);
        const dr = max - min;
        const ps = cw / (data.length - 1);
        return {w, h, cw, ch, dr, ps, min};
    }

    function updatePathPoints() {
        const d = getDims();
        pathPoints = data.map((v, i) => {
            const x = padding + i * d.ps;
            const t = d.dr === 0 ? 0 : (v - d.min) / d.dr;
            const y = d.h - padding - t * d.ch;
            return {x, y, v, date: dates[i]};
        });
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

    function drawPlot() {
        const d = getDims();
        ctx.clearRect(0, 0, d.w, d.h);
        ctx.fillStyle = '#FFFFFF';
        ctx.fillRect(0, 0, d.w, d.h);

        drawAxes();

        ctx.beginPath();
        ctx.moveTo(padding, d.h - padding);
        pathPoints.forEach(p => ctx.lineTo(p.x, p.y));
        ctx.lineTo(pathPoints[pathPoints.length - 1].x, d.h - padding);
        ctx.closePath();
        ctx.fillStyle = 'rgba(43, 108, 176, 0.2)';
        ctx.fill();

        ctx.beginPath();
        pathPoints.forEach((p, i) => {
            if (i === 0) ctx.moveTo(p.x, p.y);
            else ctx.lineTo(p.x, p.y);
        });
        ctx.strokeStyle = '#2B6CB0';
        ctx.lineWidth = 2;
        ctx.stroke();
    }

    function showTooltip(pt) {
        const r = canvas.getBoundingClientRect();
        tooltipEl.innerHTML = `Date: ${pt.date}<br>Value: ${pt.v.toFixed(2)}`;
        tooltipEl.style.left = `${r.left + pt.x}px`;
        tooltipEl.style.top = `${r.top + pt.y}px`;
        tooltipEl.style.display = 'block';
    }

    function hideTooltip() {
        tooltipEl.style.display = 'none';
    }

    function onMouseMove(e) {
        const r = canvas.getBoundingClientRect();
        const mx = e.clientX - r.left;
        const my = e.clientY - r.top;
        let found = null;
        const rr2 = 100;
        for (const p of pathPoints) {
            const dx = p.x - mx;
            const dy = p.y - my;
            if (dx * dx + dy * dy <= rr2) {
                found = p;
                break;
            }
        }
        drawPlot();
        if (found) showTooltip(found);
        else hideTooltip();
    }

    resizeCanvas();
    updatePathPoints();
    drawPlot();

    canvas.addEventListener('mousemove', onMouseMove);
    window.addEventListener('resize', () => {
        resizeCanvas();
        updatePathPoints();
        drawPlot();
        hideTooltip();
    });
}

function drawMiniChart(canvasId, tooltipId, data, dates) {
    const canvas = document.getElementById(canvasId);
    const tooltipEl = document.getElementById(tooltipId);
    const ctx = canvas.getContext('2d');
    const padding = 30;
    let pathPoints = [];

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
        const max = Math.max(...data);
        const min = Math.min(...data);
        const dr = max - min;
        const ps = cw / (data.length - 1);
        return {w, h, cw, ch, dr, ps, min};
    }

    function updatePathPoints() {
        const d = getDims();
        pathPoints = data.map((v, i) => {
            const x = padding + i * d.ps;
            const t = d.dr === 0 ? 0 : (v - d.min) / d.dr;
            const y = d.h - padding - t * d.ch;
            return {x, y, v, date: dates[i]};
        });
    }

    function drawAxes() {
        const d = getDims();
        ctx.beginPath();
        ctx.moveTo(padding, padding);
        ctx.lineTo(padding, d.h - padding);
        ctx.lineTo(d.w - padding, d.h - padding);
        ctx.strokeStyle = '#1D3557';
        ctx.lineWidth = 1.5;
        ctx.stroke();
    }

    function drawPlot() {
        const d = getDims();
        ctx.clearRect(0, 0, d.w, d.h);
        ctx.fillStyle = '#FFFFFF';
        ctx.fillRect(0, 0, d.w, d.h);

        drawAxes();

        ctx.beginPath();
        ctx.moveTo(padding, d.h - padding);
        pathPoints.forEach(p => ctx.lineTo(p.x, p.y));
        ctx.lineTo(pathPoints[pathPoints.length - 1].x, d.h - padding);
        ctx.closePath();
        ctx.fillStyle = 'rgba(43, 108, 176, 0.15)';
        ctx.fill();

        ctx.beginPath();
        pathPoints.forEach((p, i) => {
            if (i === 0) ctx.moveTo(p.x, p.y);
            else ctx.lineTo(p.x, p.y);
        });
        ctx.strokeStyle = '#2B6CB0';
        ctx.lineWidth = 1.5;
        ctx.stroke();
    }

    function showTooltip(pt) {
        const r = canvas.getBoundingClientRect();
        tooltipEl.innerHTML = `Date: ${pt.date}<br>Value: ${pt.v.toFixed(4)}`;
        tooltipEl.style.left = `${r.left + pt.x}px`;
        tooltipEl.style.top = `${r.top + pt.y}px`;
        tooltipEl.style.display = 'block';
    }

    function hideTooltip() {
        tooltipEl.style.display = 'none';
    }

    function onMouseMove(e) {
        const r = canvas.getBoundingClientRect();
        const mx = e.clientX - r.left;
        const my = e.clientY - r.top;
        let found = null;
        const rr2 = 64;
        for (const p of pathPoints) {
            const dx = p.x - mx;
            const dy = p.y - my;
            if (dx * dx + dy * dy <= rr2) {
                found = p;
                break;
            }
        }
        drawPlot();
        if (found) showTooltip(found);
        else hideTooltip();
    }

    resizeCanvas();
    updatePathPoints();
    drawPlot();

    canvas.addEventListener('mousemove', onMouseMove);
    window.addEventListener('resize', () => {
        resizeCanvas();
        updatePathPoints();
        drawPlot();
        hideTooltip();
    });
}