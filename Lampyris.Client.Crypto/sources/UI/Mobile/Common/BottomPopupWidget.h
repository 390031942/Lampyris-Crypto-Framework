#ifndef BOTTOMPOPUPWIDGET_H
#define BOTTOMPOPUPWIDGET_H

#include <QWidget>
#include <QPropertyAnimation>

class BottomPopupWidget : public QWidget {
    Q_OBJECT

public:
    explicit BottomPopupWidget(QWidget* parent = nullptr);

    void setContentWidget(QWidget* contentWidget); // ���ô�������
    void showPopup(QWidget* popup);                // ��ʾ��������
    void hidePopup();                              // ���ص�������

protected:
    void resizeEvent(QResizeEvent* event) override;
    bool eventFilter(QObject* watched, QEvent* event) override;
private slots:
    void onMaskClicked(); // ������ֹرմ���
private:
    QWidget*            m_parent;
    QWidget*            m_mask;      // ��ɫ����
    QWidget*            m_popup;     // ��������
    QPropertyAnimation* m_animation; // ����
    bool                m_show;
};

#endif // BOTTOMPOPUPWIDGET_H