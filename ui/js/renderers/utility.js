// Utility Efficiency & Flashes Renderer
function renderUtilityTable(utility) {
    const tbody = document.getElementById('utilityBody');
    if (!tbody) return;
    tbody.innerHTML = '';
    utility.forEach(u => {
        const tr = document.createElement('tr');
        const teamClass = u.team === 'T' ? 'team-t' : 'team-ct';
        let badgeCls = 'badge-b';
        if (u.rating.includes('S')) badgeCls = 'badge-s';
        else if (u.rating.includes('A')) badgeCls = 'badge-a';
        else if (u.rating.includes('C')) badgeCls = 'badge-c';

        tr.innerHTML = `
            <td class="${teamClass}">${u.team}</td>
            <td style="font-weight: 600;">${escapeHtml(u.name)}</td>
            <td>${u.flashes}</td>
            <td style="color: var(--accent-gold); font-weight: 700;">${u.blinded}</td>
            <td>${u.efficiency}</td>
            <td>${u.avgDuration}</td>
            <td>${u.teamFlashes > 1 ? `<span class="badge badge-danger">⚠️ ${u.teamFlashes}</span>` : u.teamFlashes}</td>
            <td style="color: var(--accent-orange); font-weight: 600;">${u.utilDmg} HP</td>
            <td><span class="badge ${badgeCls}">${u.rating}</span></td>
        `;
        tbody.appendChild(tr);
    });
}
