// Head-to-Head Duels Matrix Renderer
function renderDuelsMatrix(duels) {
    const container = document.getElementById('duelsMatrixContainer');
    if (!container) return;
    if (!duels || duels.length === 0) {
        container.innerHTML = '<div style="color: var(--text-dim); padding: 20px; text-align: center;">No duel data available</div>';
        return;
    }

    const tPlayers = Array.from(new Set(duels.map(d => d.tName)));
    const ctPlayers = Array.from(new Set(duels.map(d => d.ctName)));

    const duelMap = new Map();
    duels.forEach(d => duelMap.set(`${d.tName}___${d.ctName}`, d));

    let html = `<table class="matrix-table"><thead><tr><th>🟧 T \\ 🟦 CT</th>`;
    ctPlayers.forEach(ct => html += `<th style="color: var(--accent-blue);">${escapeHtml(ct)}</th>`);
    html += `</tr></thead><tbody>`;

    tPlayers.forEach(t => {
        const safeT = escapeHtml(t);
        html += `<tr><td style="font-weight: 700; color: var(--accent-orange);">${safeT}</td>`;
        ctPlayers.forEach(ct => {
            const safeCt = escapeHtml(ct);
            const duel = duelMap.get(`${t}___${ct}`) || { tWins: 0, ctWins: 0, totalDuels: 0, tHsPct: 0, ctHsPct: 0, avgTtkMs: 0 };
            
            let cls = 'win-yellow';
            let ttkDisplay = (duel.avgTtkMs > 0) ? `${duel.avgTtkMs}ms` : '--';
            
            if (duel.totalDuels === 0) {
                cls = '';
            } else if (duel.tWins > duel.ctWins) {
                cls = 'win-green';
            } else if (duel.ctWins > duel.tWins) {
                cls = 'win-red';
            }

            const tooltip = `${safeT} (${duel.tWins} kills, ${duel.tHsPct}% HS) vs ${safeCt} (${duel.ctWins} kills, ${duel.ctHsPct}% HS) | Avg TTK: ${ttkDisplay}`;
            const cellStyle = (duel.totalDuels === 0) 
                ? 'background: #0E1015; color: var(--text-dim); border: 1px solid var(--panel-border); opacity: 0.65;' 
                : '';

            html += `<td class="matrix-cell ${cls}" style="${cellStyle}" title="${tooltip}">
                        <div>${duel.tWins} - ${duel.ctWins}</div>
                        <div style="font-size: 9px; opacity: 0.85;">${ttkDisplay}</div>
                     </td>`;
        });
        html += `</tr>`;
    });
    html += `</tbody></table>`;
    container.innerHTML = html;
}
