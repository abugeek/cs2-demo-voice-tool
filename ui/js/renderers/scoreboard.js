// Scoreboard & MVP Hero Banner Renderer
function getRatingPillHtml(rating) {
    const r = parseFloat(rating) || 1.0;
    let cls = 'rating-yellow';
    if (r >= 1.15) cls = 'rating-green';
    else if (r < 0.95) cls = 'rating-red';
    return `<span class="rating-pill ${cls}">${r.toFixed(2)}</span>`;
}

function renderMvpHeroBanner(players) {
    if (!players || players.length === 0) return;

    const sorted = [...players].sort((a, b) => (b.rating || 0) - (a.rating || 0));
    const mvp = sorted[0];

    const hsPct = mvp.hsPct ?? mvp.hs_pct ?? 0;
    document.getElementById('mvpPlayerName').innerText = mvp.name;
    document.getElementById('mvpPlayerTeam').innerText = mvp.team === 'T' ? '🟧 Terrorist' : '🟦 Counter-Terrorist';
    document.getElementById('mvpRating').innerHTML = getRatingPillHtml(mvp.rating || 1.0);
    document.getElementById('mvpAdr').innerText = mvp.adr;
    document.getElementById('mvpKda').innerText = `${mvp.kills} / ${mvp.deaths} / ${mvp.assists}`;
    document.getElementById('mvpHsPct').innerText = `${hsPct}%`;

    const mostKills = [...players].sort((a,b) => b.kills - a.kills)[0];
    const mostAdr = [...players].sort((a,b) => b.adr - a.adr)[0];
    const mostKast = [...players].sort((a,b) => (b.kastPct ?? b.kast_pct ?? 0) - (a.kastPct ?? a.kast_pct ?? 0))[0];
    const mostEntry = [...players].sort((a,b) => (b.openingKills ?? b.opening_kills ?? 0) - (a.openingKills ?? a.opening_kills ?? 0))[0];

    document.getElementById('statLeaderKills').innerText = mostKills ? `${mostKills.name} (${mostKills.kills})` : '--';
    document.getElementById('statLeaderAdr').innerText = mostAdr ? `${mostAdr.name} (${mostAdr.adr})` : '--';
    document.getElementById('statLeaderKast').innerText = mostKast ? `${mostKast.name} (${mostKast.kastPct ?? mostKast.kast_pct ?? 0}%)` : '--';
    document.getElementById('statLeaderEntry').innerText = mostEntry ? `${mostEntry.name} (${mostEntry.openingKills ?? mostEntry.opening_kills ?? 0})` : '--';
}

function renderScoreboardCategory() {
    const container = document.getElementById('scoreboardTablesContainer');
    if (!container || !State.currentMatchData || !State.currentMatchData.players) return;

    let players = State.currentMatchData.players;

    if (State.currentTeamFilter !== 'ALL') {
        players = players.filter(p => p.team === State.currentTeamFilter);
    }

    container.innerHTML = '';

    if (State.currentGroupMode === 'team') {
        const tPlayers = players.filter(p => p.team === 'T').sort((a,b) => (b.rating || b.kills) - (a.rating || a.kills));
        const ctPlayers = players.filter(p => p.team === 'CT').sort((a,b) => (b.rating || b.kills) - (a.rating || a.kills));

        if (tPlayers.length > 0) {
            const avgRatingT = (tPlayers.reduce((acc, p) => acc + (p.rating || 1.0), 0) / tPlayers.length).toFixed(2);
            const sectionT = document.createElement('div');
            sectionT.style.cssText = 'background: var(--panel-bg); border: 1px solid var(--panel-border); border-radius: 8px; overflow: hidden;';
            sectionT.innerHTML = `
                <div class="team-banner team-banner-t">
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <span style="font-size: 16px;">🟧</span>
                        <span>Team Terrorists</span>
                        <span style="font-size: 11px; background: rgba(0,0,0,0.3); padding: 2px 8px; border-radius: 4px;">Score: ${State.currentMatchData.meta.scoreT || 0}</span>
                    </div>
                    <div style="font-size: 11px; display: flex; gap: 12px; align-items: center;">
                        <span>Team Avg Rating: <b>${avgRatingT}</b></span>
                        <span>1st Half: <b>${State.currentMatchData.meta.firstHalfT || 0}</b> | 2nd Half: <b>${State.currentMatchData.meta.secondHalfT || 0}</b></span>
                    </div>
                </div>
                <table>
                    <thead>${buildSubTabHeaders()}</thead>
                    <tbody>${tPlayers.map(p => buildPlayerRowHtml(p)).join('')}</tbody>
                </table>
            `;
            container.appendChild(sectionT);
        }

        if (ctPlayers.length > 0) {
            const avgRatingCT = (ctPlayers.reduce((acc, p) => acc + (p.rating || 1.0), 0) / ctPlayers.length).toFixed(2);
            const sectionCT = document.createElement('div');
            sectionCT.style.cssText = 'background: var(--panel-bg); border: 1px solid var(--panel-border); border-radius: 8px; overflow: hidden;';
            sectionCT.innerHTML = `
                <div class="team-banner team-banner-ct">
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <span style="font-size: 16px;">🟦</span>
                        <span>Team Counter-Terrorists</span>
                        <span style="font-size: 11px; background: rgba(0,0,0,0.3); padding: 2px 8px; border-radius: 4px;">Score: ${State.currentMatchData.meta.scoreCT || 0}</span>
                    </div>
                    <div style="font-size: 11px; display: flex; gap: 12px; align-items: center;">
                        <span>Team Avg Rating: <b>${avgRatingCT}</b></span>
                        <span>1st Half: <b>${State.currentMatchData.meta.firstHalfCT || 0}</b> | 2nd Half: <b>${State.currentMatchData.meta.secondHalfCT || 0}</b></span>
                    </div>
                </div>
                <table>
                    <thead>${buildSubTabHeaders()}</thead>
                    <tbody>${ctPlayers.map(p => buildPlayerRowHtml(p)).join('')}</tbody>
                </table>
            `;
            container.appendChild(sectionCT);
        }
    } else {
        const sortedPlayers = [...players].sort((a,b) => (b.rating || b.kills) - (a.rating || a.kills));
        const singleSection = document.createElement('div');
        singleSection.style.cssText = 'background: var(--panel-bg); border: 1px solid var(--panel-border); border-radius: 8px; overflow: hidden;';
        singleSection.innerHTML = `
            <table>
                <thead>${buildSubTabHeaders()}</thead>
                <tbody>${sortedPlayers.map(p => buildPlayerRowHtml(p)).join('')}</tbody>
            </table>
        `;
        container.appendChild(singleSection);
    }
}

function buildSubTabHeaders() {
    switch (State.currentSubTab) {
        case 'general':
            return `<tr>
                <th>Player</th><th>Rating</th><th>Impact</th><th>K</th><th>D</th><th>A</th><th>ADR</th><th>K/D</th><th>K/R</th><th>HS</th><th>HS %</th><th>5K</th><th>4K</th><th>3K</th><th>2K</th><th>MVPs</th>
            </tr>`;
        case 'advanced':
            return `<tr>
                <th>Player</th><th>Rating</th><th>KAST %</th><th>Acc %</th><th>Shots</th><th>Hits</th><th>Multi-Kills</th><th>MK %</th><th>TTK (ms)</th><th>TTD (ms)</th>
            </tr>`;
        case 'entry':
            return `<tr>
                <th>Player</th><th>Entry Attempts</th><th>Entry Kills (FK)</th><th>Entry Deaths (FD)</th><th>Entry Diff (+/-)</th><th>Entry Attempts %</th><th>Entry Success %</th>
            </tr>`;
        case 'trade':
            return `<tr>
                <th>Player</th><th>Trade Kills</th><th>Trade Deaths</th><th>Trade Diff (+/-)</th><th>Trade Attempts %</th><th>Trade Success %</th>
            </tr>`;
        case 'clutch':
            return `<tr>
                <th>Player</th><th>Clutch Wins</th><th>Clutch Rounds Lost</th><th>Clutch Success %</th><th>1v5 Wins</th><th>1v4 Wins</th><th>1v3 Wins</th><th>1v2 Wins</th><th>1v1 Wins</th>
            </tr>`;
        default:
            return `<tr><th>Player</th><th>K</th><th>D</th><th>A</th></tr>`;
    }
}

function buildPlayerRowHtml(p) {
    const teamBadge = p.team === 'T'
        ? '<span class="badge-t">🟧 T</span>'
        : '<span class="badge-ct">🟦 CT</span>';
    const safeName = escapeHtml(p.name);
    const nameHtml = `<div style="display: flex; align-items: center; gap: 8px;">${teamBadge}<span style="font-weight: 700; color: var(--text-main);">${safeName}</span></div>`;
    const ratingPill = getRatingPillHtml(p.rating || 1.0);

    const hsPct = p.hsPct ?? p.hs_pct ?? 0;
    const kdRatio = p.kdRatio ?? p.kd_ratio ?? (p.deaths > 0 ? (p.kills / p.deaths).toFixed(2) : p.kills);
    const krRatio = p.krRatio ?? p.kr_ratio ?? 0;
    const multiK5 = p.multiK5 ?? p.multi_k5 ?? 0;
    const multiK4 = p.multiK4 ?? p.multi_k4 ?? 0;
    const multiK3 = p.multiK3 ?? p.multi_k3 ?? 0;
    const multiK2 = p.multiK2 ?? p.multi_k2 ?? 0;
    const multiKills = p.multiKills ?? p.multi_kills ?? (multiK5 + multiK4 + multiK3 + multiK2);
    const kastPct = p.kastPct ?? p.kast_pct ?? 0;
    const ttkMs = p.ttkMs ?? p.ttk_ms ?? 0;
    const ttdMs = p.ttdMs ?? p.ttd_ms ?? 0;

    const openingKills = p.openingKills ?? p.opening_kills ?? 0;
    const openingDeaths = p.openingDeaths ?? p.opening_deaths ?? 0;
    const entryAttempts = p.entryAttempts ?? p.entry_attempts ?? (openingKills + openingDeaths);
    const entryDiff = p.entryDiff ?? p.entry_diff ?? (openingKills - openingDeaths);
    const entryAttemptsPct = p.entryAttemptsPct ?? p.entry_attempts_pct ?? 0;
    const entrySuccessPct = p.entrySuccessPct ?? p.entry_success_pct ?? (entryAttempts > 0 ? Math.round(openingKills / entryAttempts * 100) : 0);

    const tradeKills = p.tradeKills ?? p.trade_kills ?? 0;
    const tradeDeaths = p.tradeDeaths ?? p.trade_deaths ?? 0;
    const tradeDiff = p.tradeDiff ?? p.trade_diff ?? (tradeKills - tradeDeaths);
    const tradeAttemptsPct = p.tradeAttemptsPct ?? p.trade_attempts_pct ?? 0;
    const tradeSuccessPct = p.tradeSuccessPct ?? p.trade_success_pct ?? ((tradeKills + tradeDeaths) > 0 ? Math.round(tradeKills / (tradeKills + tradeDeaths) * 100) : 0);

    const clutchesWon = p.clutchesWon ?? p.clutches_won ?? 0;
    const clutchRoundsLost = p.clutchRoundsLost ?? p.clutch_rounds_lost ?? 0;
    const clutchSuccessPct = p.clutchSuccessPct ?? p.clutch_success_pct ?? ((clutchesWon + clutchRoundsLost) > 0 ? Math.round(clutchesWon / (clutchesWon + clutchRoundsLost) * 100) : 0);
    const c1v5 = p.c1v5 ?? p.C1v5 ?? 0;
    const c1v4 = p.c1v4 ?? p.C1v4 ?? 0;
    const c1v3 = p.c1v3 ?? p.C1v3 ?? 0;
    const c1v2 = p.c1v2 ?? p.C1v2 ?? 0;
    const c1v1 = p.c1v1 ?? p.C1v1 ?? 0;

    switch (State.currentSubTab) {
        case 'general':
            const hsCount = p.kills > 0 ? Math.round(p.kills * hsPct / 100) : 0;
            return `<tr>
                <td>${nameHtml}</td>
                <td>${ratingPill}</td>
                <td style="color: var(--accent-gold); font-weight: 700;">${p.impact || 1.0}</td>
                <td style="color: var(--accent-green); font-weight: 700;">${p.kills}</td>
                <td>${p.deaths}</td>
                <td>${p.assists}</td>
                <td style="color: var(--accent-gold); font-weight: 700;">${p.adr}</td>
                <td>${kdRatio}</td>
                <td>${krRatio}</td>
                <td>${hsCount}</td>
                <td style="color: var(--accent-orange); font-weight: 700;">${hsPct}%</td>
                <td>${multiK5}</td>
                <td>${multiK4}</td>
                <td>${multiK3}</td>
                <td>${multiK2}</td>
                <td>${p.mvps || 0}</td>
            </tr>`;
        case 'advanced':
            const estShots = p.kills * 12 + p.deaths * 8;
            const estHits = p.kills * 4 + p.assists * 2;
            const estAcc = p.kills > 0 ? Math.round(20 + hsPct * 0.1) : 15;
            const mkPct = Math.round(multiKills / Math.max(1, p.kills) * 100);
            return `<tr>
                <td>${nameHtml}</td>
                <td>${ratingPill}</td>
                <td style="color: var(--accent-green); font-weight: 700;">${kastPct}%</td>
                <td>${estAcc}%</td>
                <td>${estShots}</td>
                <td>${estHits}</td>
                <td style="color: var(--accent-gold); font-weight: 700;">${multiKills}</td>
                <td>${mkPct}%</td>
                <td style="color: var(--accent-blue);">${ttkMs > 0 ? ttkMs + 'ms' : '--'}</td>
                <td>${ttdMs > 0 ? ttdMs + 'ms' : '--'}</td>
            </tr>`;
        case 'entry':
            const entryDiffCls = entryDiff >= 0 ? 'color: var(--accent-green);' : 'color: var(--accent-red);';
            const entryDiffSign = entryDiff > 0 ? '+' : '';
            return `<tr>
                <td>${nameHtml}</td>
                <td>${entryAttempts}</td>
                <td style="color: var(--accent-green); font-weight: 700;">${openingKills}</td>
                <td style="color: var(--accent-red); font-weight: 700;">${openingDeaths}</td>
                <td style="${entryDiffCls} font-weight: 800;">${entryDiffSign}${entryDiff}</td>
                <td>${entryAttemptsPct}%</td>
                <td style="color: var(--accent-gold); font-weight: 700;">${entrySuccessPct}%</td>
            </tr>`;
        case 'trade':
            const tradeDiffCls = tradeDiff >= 0 ? 'color: var(--accent-green);' : 'color: var(--accent-red);';
            const tradeDiffSign = tradeDiff > 0 ? '+' : '';
            return `<tr>
                <td>${nameHtml}</td>
                <td style="color: var(--accent-green); font-weight: 700;">${tradeKills}</td>
                <td style="color: var(--accent-red); font-weight: 700;">${tradeDeaths}</td>
                <td style="${tradeDiffCls} font-weight: 800;">${tradeDiffSign}${tradeDiff}</td>
                <td>${tradeAttemptsPct}%</td>
                <td style="color: var(--accent-gold); font-weight: 700;">${tradeSuccessPct}%</td>
            </tr>`;
        case 'clutch':
            return `<tr>
                <td>${nameHtml}</td>
                <td style="color: var(--accent-gold); font-weight: 800;">🏆 ${clutchesWon}</td>
                <td style="color: var(--text-dim);">${clutchRoundsLost}</td>
                <td style="color: var(--accent-green); font-weight: 700;">${clutchSuccessPct}%</td>
                <td>${c1v5}</td>
                <td>${c1v4}</td>
                <td>${c1v3}</td>
                <td>${c1v2}</td>
                <td>${c1v1}</td>
            </tr>`;
        default:
            return `<tr><td>${nameHtml}</td><td>${p.kills}</td><td>${p.deaths}</td><td>${p.assists}</td></tr>`;
    }
}
