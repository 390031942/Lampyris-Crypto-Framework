#ifndef BOTTOMPOPUPWIDGET_H
#define BOTTOMPOPUPWIDGET_H

#include <QWidget>
#include <QPropertyAnimation>

class BottomPopupWidget : public QWidget {
    Q_OBJECT

public:
    explicit BottomPopupWidget(QWidget* parent = nullptr);

    void setContentWidget(QWidget* contentWidget); // 设置窗口内容
    void showPopup(QWidget* popup);                // 显示弹出窗口
    void hidePopup();                              // 隐藏弹出窗口

protected:
    void resizeEvent(QResizeEvent* event) override;
    bool eventFilter(QObject* watched, QEvent* event) override;
private slots:
    void onMaskClicked(); // 点击遮罩关闭窗口
private:
    QWidget*            m_parent;
    QWidget*            m_mask;      // 灰色遮罩
    QWidget*            m_popup;     // 弹出窗口
    QPropertyAnimation* m_animation; // 动画
    bool                m_show;
};

#endif // BOTTOMPOPUPWIDGET_H