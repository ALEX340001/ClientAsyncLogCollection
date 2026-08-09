const providersContainer = document.getElementById('providers-container');
    const addProviderBtn = document.getElementById('add-provider');
    const generateCliBtn = document.getElementById('generate-cli');
    const generateJsonBtn = document.getElementById('generate-json');
    const cliFeedback = document.getElementById('cli-feedback');
    const jsonFeedback = document.getElementById('json-feedback');
    function toggleCustomLog(selectElement) {
        const row = selectElement.closest('.provider-row');
        const customInput = row.querySelector('.provider-log-custom');
        if (selectElement.value === '__custom__') {
            customInput.style.display = 'inline-block';
            customInput.required = true;
        } else {
            customInput.style.display = 'none';
            customInput.required = false;
        }
    }
    function addProviderRow(name = '', log = 'Application', customLog = '') {
        const row = document.createElement('div');
        row.className = 'provider-row';
        row.innerHTML = `
            <input type="text" placeholder="Имя провайдера" class="provider-name" value="${name}">
            <select class="provider-log">
                <option value="Application" ${log === 'Application' ? 'selected' : ''}>Application</option>
                <option value="System" ${log === 'System' ? 'selected' : ''}>System</option>
                <option value="Security" ${log === 'Security' ? 'selected' : ''}>Security</option>
                <option value="__custom__" ${log === '__custom__' ? 'selected' : ''}>Другой...</option>
            </select>
            <input type="text" class="provider-log-custom" placeholder="Свой журнал" style="display:${log === '__custom__' ? 'inline-block' : 'none'}; width:120px;" value="${customLog}">
            <button class="btn btn-small remove-provider">✕</button>
        `;
        providersContainer.appendChild(row);
        row.querySelector('.provider-log').addEventListener('change', function() { toggleCustomLog(this); });
        row.querySelector('.remove-provider').addEventListener('click', function() {
            if (document.querySelectorAll('.provider-row').length > 1) {
                row.remove();
            }
        });
    }
    document.querySelectorAll('.provider-row').forEach(row => {
        const select = row.querySelector('.provider-log');
        select.addEventListener('change', function() { toggleCustomLog(this); });
        const removeBtn = row.querySelector('.remove-provider');
        if (removeBtn) {
            removeBtn.style.display = 'inline-block';
            removeBtn.addEventListener('click', function() {
                if (document.querySelectorAll('.provider-row').length > 1) {
                    row.remove();
                }
            });
        }
    });
    addProviderBtn.addEventListener('click', () => addProviderRow());
    function getProviders() {
        const rows = document.querySelectorAll('.provider-row');
        const providers = [];
        rows.forEach(row => {
            const name = row.querySelector('.provider-name').value.trim();
            if (!name) return;
            const logSelect = row.querySelector('.provider-log');
            let log = logSelect.value;
            if (log === '__custom__') {
                log = row.querySelector('.provider-log-custom').value.trim();
                if (!log) log = 'Application';
            }
            providers.push({ Name: name, Log: log });
        });
        return providers;
    }
    function getFlags() {
        const checkboxes = document.querySelectorAll('.flag:checked');
        return Array.from(checkboxes).map(cb => parseInt(cb.value));
    }
    function generateCLI() {
        const exe = document.getElementById('exe-name').value.trim() || 'ClientAsyncLogCollection.exe';
        const providers = getProviders();
        const flags = getFlags();
        const format = document.getElementById('format').value;
        const defaultFolder = document.getElementById('defaultfolder').value.trim();
        const userFolder = document.getElementById('userfolder').value.trim();
        const days = document.getElementById('daystocollect').value;
        let cmd = exe;
        if (providers.length > 0) {
            const appNames = providers.map(p => p.Name).join(',');
            cmd += ` --apps "${appNames}"`;
        }
        if (flags.length > 0 && flags.length < 5) {
            cmd += ` --flags ${flags.join(',')}`;
        }
        if (format && format !== 'json') {
            cmd += ` -f ${format}`;
        }
        if (userFolder) {
            cmd += ` --pl "${userFolder}"`;
        } else if (defaultFolder) {
            cmd += ` --defaultFolder "${defaultFolder}"`;
        }
        if (days && days !== '1') {
            cmd += ` -t ${days}`;
        }
        return cmd;
    }
    function generateJSON() {
        const providers = getProviders();
        const flags = getFlags();
        const format = document.getElementById('format').value;
        const defaultFolder = document.getElementById('defaultfolder').value.trim();
        const userFolder = document.getElementById('userfolder').value.trim();
        const days = document.getElementById('daystocollect').value;
        const configName = document.getElementById('configname').value.trim() || 'MyConfig';
        const config = {
    DefaultFolderPath: defaultFolder || "Logs",
    UserFolderPath: userFolder || "",
    Format: format,
    EnabledApplications: providers,
    Flags: flags.length === 5 ? [] : flags,
    DaysToCollect: parseInt(days) || 1
};
        return JSON.stringify(config, null, 2);
    }
    function copyToClipboard(text, feedbackElement) {
        navigator.clipboard.writeText(text).then(() => {
            feedbackElement.classList.add('show');
            setTimeout(() => feedbackElement.classList.remove('show'), 2000);
        }).catch(() => {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
            feedbackElement.classList.add('show');
            setTimeout(() => feedbackElement.classList.remove('show'), 2000);
        });
    }
    function downloadJSON(jsonContent) {
    const blob = new Blob([jsonContent], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'appsettings.json';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}
document.getElementById('download-json').addEventListener('click', () => {
    const json = generateJSON();
    downloadJSON(json);
});
    generateCliBtn.addEventListener('click', () => {
        copyToClipboard(generateCLI(), cliFeedback);
    });
    generateJsonBtn.addEventListener('click', () => {
        copyToClipboard(generateJSON(), jsonFeedback);
    });