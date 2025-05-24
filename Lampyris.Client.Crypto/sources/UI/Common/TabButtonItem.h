#pragma once
// Project Include(s)
#include <QWidget>
#include <QPainter>
#include <QMouseEvent>

enum TabButtonDisplayMode {
    NORMAL = 0, // 普通效果:未选中灰色，选中为白色
    UNDERLINE = 1, // 选中后下方有黄色线
    BACKGROUND = 2, // 选中后有圆角矩形
};

class TabButtonItem : public QWidget {
    Q_OBJECT
public:
    explicit             TabButtonItem(const QString &text, QWidget *parent = nullptr);
    void                 setSelected(bool selected);
    void                 setDisplayMode(TabButtonDisplayMode mode);
    QString              text() const;
signals:
    void                 clicked();
protected:               
    void                 paintEvent(QPaintEvent *event) override;
    void                 mousePressEvent(QMouseEvent* event) override;
private:                 
    QString              m_text;
    bool                 m_selected;
    TabButtonDisplayMode m_mode;
};