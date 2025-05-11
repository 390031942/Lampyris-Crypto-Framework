#pragma once

// QT Include(s)
#include <QApplication>
#include <QMainWindow>
#include <QToolBar>
#include <QToolButton>
#include <QIcon>
#include <QDebug>

class MainToolButton : public QToolButton {
    Q_OBJECT

public:
              MainToolButton(const QString& defaultIconPath,
                             const QString& hoverIconPath,
                             const QString& checkedIconPath,
                             const QString& text,
                             QWidget* parent = nullptr);

protected:
    void      enterEvent(QEnterEvent* event) override;
    void      leaveEvent(QEvent* event) override;
private slots:
    void      updateIcon(bool checked);
private:      
    QIcon     m_defaultIcon;  // Ĭ��ͼ��
    QIcon     m_hoverIcon;    // �����ͣʱ��ͼ��
    QIcon     m_checkedIcon;  // ��ťѡ��ʱ��ͼ��
};