#pragma once
// QT Include(s)
#include <QLineEdit>

class SymbolSearchResultWidget;

/*
 * ���ư�QLineEdit
 * 1) ֧��Enter����������������� ��ȷ���������
 * 2) ֧������ʱ�·���ʾ����ʴ���
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

    // ��ʷ��¼����ʴ���
    SymbolSearchResultWidget* m_historyWidget;
};