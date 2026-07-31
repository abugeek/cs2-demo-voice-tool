// Clutches & Hero Plays Table Renderer
function renderClutchesTable(clutches) {
    const tbody = document.getElementById('clutchesBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    const total = clutches ? clutches.length : 0;
    let c1v12 = 0;
    let c1v3plus = 0;
    const playerCounts = {};

    document.getElementById('statTotalClutches').innerText = total;

    if (!clutches || clutches.length === 0) {
        document.getElementById('stat1v11v2').innerText = '0';
        document.getElementById('stat1v3Plus').innerText = '0';
        document.getElementById('statClutchMvp').innerText = 'None';
        tbody.innerHTML = '<tr><td colspan="7" style="text-align: center; color: var(--text-dim); padding: 24px;">No clutches recorded in this match demo</td></tr>';
        return;
    }

    clutches.forEach(c => {
        if (c.vsCount <= 2) c1v12++;
        else c1v3plus++;

        playerCounts[c.playerName] = (playerCounts[c.playerName] || 0) + 1;

        const tr = document.createElement('tr');
        const isT = c.team === 'T';
        const teamBadge = isT ? '<span class="badge-t">🟧 T</span>' : '<span class="badge-ct">🟦 CT</span>';
        
        let badgeCls = 'badge-b';
        if (c.vsCount >= 3) badgeCls = 'badge-s';
        else if (c.vsCount === 2) badgeCls = 'badge-a';

        const safePlayerName = escapeHtml(c.playerName);
        const oppsRaw = c.opponents && c.opponents.length > 0 ? c.opponents.map(o => escapeHtml(o)).join(', ') : 'Opponents';
        const safeWinType = escapeHtml(c.winType);
        const safeDetails = escapeHtml(c.details);
        const safeClutchType = escapeHtml(c.clutchType);

        tr.innerHTML = `
            <td style="font-weight: 700; color: var(--accent-gold);">Round ${c.roundNum}</td>
            <td>${teamBadge}</td>
            <td style="font-weight: 700; color: var(--text-main);">${safePlayerName}</td>
            <td><span class="badge ${badgeCls}" style="font-size: 11px;">🏆 ${safeClutchType}</span></td>
            <td style="color: var(--accent-green); font-weight: 600;">${safeWinType}</td>
            <td style="color: var(--text-dim); font-size: 11px;">${oppsRaw}</td>
            <td style="font-size: 11px;">${safeDetails}</td>
        `;
        tbody.appendChild(tr);
    });

    document.getElementById('stat1v11v2').innerText = c1v12;
    document.getElementById('stat1v3Plus').innerText = c1v3plus;

    let topMvp = 'None';
    let maxC = 0;
    for (const [name, cnt] of Object.entries(playerCounts)) {
        if (cnt > maxC) { maxC = cnt; topMvp = `${escapeHtml(name)} (${cnt})`; }
    }
    document.getElementById('statClutchMvp').innerText = topMvp;
}
