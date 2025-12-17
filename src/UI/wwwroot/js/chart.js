const chartColors = {
    primary: '#6366F1',
    primaryLight: '#818CF8',
    primaryDark: '#4F46E5',
    secondary: '#14B8A6',
    success: '#22C55E',
    error: '#EF4444',
    background: '#FFFFFF',
    gridLine: '#E2E8F0',
    text: '#0F172A',
    textSecondary: '#64748B',
    gradientStart: 'rgba(99, 102, 241, 0.25)',
    gradientEnd: 'rgba(99, 102, 241, 0.02)'
};

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
        return {w, h, cw, ch, dr, ps, min, max};
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
        
        // Draw grid lines
        ctx.strokeStyle = chartColors.gridLine;
        ctx.lineWidth = 1;
        
        // Horizontal grid lines
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
        ctx.strokeStyle = chartColors.gridLine;
        ctx.lineWidth = 2;
        ctx.stroke();
        
        // Labels
        ctx.font = '600 14px Inter, sans-serif';
        ctx.fillStyle = chartColors.textSecondary;
        ctx.fillText('Date', d.w / 2 - 15, d.h - 12);
        ctx.save();
        ctx.translate(18, d.h / 2 + 20);
        ctx.rotate(-Math.PI / 2);
        ctx.fillText('Value', 0, 0);
        ctx.restore();
    }

    function drawPlot() {
        const d = getDims();
        ctx.clearRect(0, 0, d.w, d.h);
        
        // White background with subtle shadow effect
        ctx.fillStyle = chartColors.background;
        ctx.fillRect(0, 0, d.w, d.h);

        drawAxes();

        // Create gradient fill
        const gradient = ctx.createLinearGradient(0, padding, 0, d.h - padding);
        gradient.addColorStop(0, chartColors.gradientStart);
        gradient.addColorStop(1, chartColors.gradientEnd);

        // Fill area under curve
        ctx.beginPath();
        ctx.moveTo(padding, d.h - padding);
        pathPoints.forEach(p => ctx.lineTo(p.x, p.y));
        ctx.lineTo(pathPoints[pathPoints.length - 1].x, d.h - padding);
        ctx.closePath();
        ctx.fillStyle = gradient;
        ctx.fill();

        // Draw the line with gradient stroke
        ctx.beginPath();
        pathPoints.forEach((p, i) => {
            if (i === 0) ctx.moveTo(p.x, p.y);
            else ctx.lineTo(p.x, p.y);
        });
        ctx.strokeStyle = chartColors.primary;
        ctx.lineWidth = 3;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        ctx.stroke();
        
        // Draw points
        pathPoints.forEach((p, i) => {
            if (i % Math.max(1, Math.floor(pathPoints.length / 20)) === 0 || i === pathPoints.length - 1) {
                ctx.beginPath();
                ctx.arc(p.x, p.y, 4, 0, Math.PI * 2);
                ctx.fillStyle = chartColors.background;
                ctx.fill();
                ctx.strokeStyle = chartColors.primary;
                ctx.lineWidth = 2;
                ctx.stroke();
            }
        });
    }

    function showTooltip(pt) {
        const r = canvas.getBoundingClientRect();
        tooltipEl.innerHTML = `<strong>${pt.date}</strong><br>Value: <span style="color: ${chartColors.primary}">${pt.v.toFixed(4)}</span>`;
        tooltipEl.style.left = `${r.left + pt.x}px`;
        tooltipEl.style.top = `${r.top + pt.y - 10}px`;
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
        let minDist = Infinity;
        
        for (const p of pathPoints) {
            const dx = p.x - mx;
            const dy = p.y - my;
            const dist = dx * dx + dy * dy;
            if (dist < minDist && dist <= 400) {
                minDist = dist;
                found = p;
            }
        }
        
        drawPlot();
        
        if (found) {
            // Draw highlight point
            ctx.beginPath();
            ctx.arc(found.x, found.y, 8, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(99, 102, 241, 0.2)';
            ctx.fill();
            ctx.beginPath();
            ctx.arc(found.x, found.y, 5, 0, Math.PI * 2);
            ctx.fillStyle = chartColors.primary;
            ctx.fill();
            
            showTooltip(found);
        } else {
            hideTooltip();
        }
    }

    resizeCanvas();
    updatePathPoints();
    drawPlot();

    canvas.addEventListener('mousemove', onMouseMove);
    canvas.addEventListener('mouseleave', () => {
        drawPlot();
        hideTooltip();
    });
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
    const padding = 25;
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
        ctx.strokeStyle = chartColors.gridLine;
        ctx.lineWidth = 1;
        ctx.stroke();
    }

    function drawPlot() {
        const d = getDims();
        ctx.clearRect(0, 0, d.w, d.h);
        ctx.fillStyle = chartColors.background;
        ctx.fillRect(0, 0, d.w, d.h);

        drawAxes();

        // Gradient fill
        const gradient = ctx.createLinearGradient(0, padding, 0, d.h - padding);
        gradient.addColorStop(0, chartColors.gradientStart);
        gradient.addColorStop(1, chartColors.gradientEnd);

        ctx.beginPath();
        ctx.moveTo(padding, d.h - padding);
        pathPoints.forEach(p => ctx.lineTo(p.x, p.y));
        ctx.lineTo(pathPoints[pathPoints.length - 1].x, d.h - padding);
        ctx.closePath();
        ctx.fillStyle = gradient;
        ctx.fill();

        // Line
        ctx.beginPath();
        pathPoints.forEach((p, i) => {
            if (i === 0) ctx.moveTo(p.x, p.y);
            else ctx.lineTo(p.x, p.y);
        });
        ctx.strokeStyle = chartColors.primary;
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';
        ctx.stroke();
    }

    function showTooltip(pt) {
        const r = canvas.getBoundingClientRect();
        tooltipEl.innerHTML = `<strong>${pt.date}</strong><br>Value: ${pt.v.toFixed(4)}`;
        tooltipEl.style.left = `${r.left + pt.x}px`;
        tooltipEl.style.top = `${r.top + pt.y - 10}px`;
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
        if (found) {
            ctx.beginPath();
            ctx.arc(found.x, found.y, 5, 0, Math.PI * 2);
            ctx.fillStyle = chartColors.primary;
            ctx.fill();
            showTooltip(found);
        }
        else hideTooltip();
    }

    resizeCanvas();
    updatePathPoints();
    drawPlot();

    canvas.addEventListener('mousemove', onMouseMove);
    canvas.addEventListener('mouseleave', () => {
        drawPlot();
        hideTooltip();
    });
    window.addEventListener('resize', () => {
        resizeCanvas();
        updatePathPoints();
        drawPlot();
        hideTooltip();
    });
}