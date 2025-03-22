// Project Include(s)
#include "LauncherWidget.h"

LauncherWidget::LauncherWidget(const QString& configPath, const QStringList& args, QWidget* parent)
    : QWidget(parent), m_launcherManager(new LauncherManager(configPath, args, this)) {
    m_ui.setupUi(this);

    connect(m_launcherManager, &LauncherManager::updateStatus, this, [this](const QString& status) {
        m_ui.statusLabel->setText(status);
    });

    connect(m_launcherManager, &LauncherManager::updateProgress, this, [this](int progress) {
        m_ui.progressBar->setValue(progress);
    });

    connect(m_launcherManager, &LauncherManager::retryCountdown, this, [this](int seconds) {
        m_ui.statusLabel->setText(QString("检查更新失败，将在 %1 秒后重试...").arg(seconds));
    });

    connect(m_launcherManager, &LauncherManager::loadSucceed, this, [this]() {
        this->close();
    });

    m_launcherManager->checkForUpdates();
}
