#pragma once
// QT Include(s)
#include <QLineEdit>

class SymbolSearchResultWidget;

/*
 * 定制版QLineEdit
 * 1) 支持Enter键或鼠标点击其它区域 来确认输入完成
 * 2) 支持输入时下方显示联想词窗口
 */
class BetterLineEdit :public QLineEdit {
    Q_OBJECT
public:
	explicit                  BetterLineEdit(QWidget* parent = Q_NULLPTR);
    void                      setHistoryWidget(SymbolSearchResultWidget* historyWidget) 
    { m_historyWidget = historyWidget; }
Q_SIGNALS:
    void                      signalOutFocus();
    void                      signalInFocus();
protected:
    void                      focusOutEvent(QFocusEvent* event) override;
    void                      focusInEvent(QFocusEvent* event) override;
    void                      keyPressEvent(QKeyEvent* event) override;
private:
    bool                      m_focus = false;

    // 历史记录联想词窗口
    SymbolSearchResultWidget* m_historyWidget;
};