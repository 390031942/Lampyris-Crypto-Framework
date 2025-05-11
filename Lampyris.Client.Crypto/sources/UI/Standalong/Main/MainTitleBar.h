#pragma once

// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QHBoxLayout>
#include <QLabel>
#include <QSpacerItem>
#include <QLineEdit>
#include <QListWidget>
#include <QPushButton>
#include <QIcon>
#include <QMouseEvent>
#include <QVBoxLayout>
#include <QStyle>
#include <QDebug>
#include <QAction>

class BetterLineEdit;
class SymbolSearchResultWidget;

class MainTitleBar : public QWidget {
    Q_OBJECT
public:
    explicit                  MainTitleBar(QWidget* parent = nullptr);
    void                      setSuggestions(const QStringList& suggestions);
protected:                    
    void                      mousePressEvent(QMouseEvent* event) override;
    void                      mouseMoveEvent(QMouseEvent* event) override;
    void                      mouseReleaseEvent(QMouseEvent* event) override;
private slots:                
    void                      minimizeWindow();
    void                      maximizeWindow();
    void                      closeWindow();
    void                      updateSuggestions(const QString& text);
    void                      selectSuggestion(QListWidgetItem* item);
    void                      onSearchBoxFocusIn();
    void                      onSearchBoxFocusOut();
private:
    BetterLineEdit*           searchBox;
    QListWidget*              suggestionList;
    QStringList               suggestions;
    SymbolSearchResultWidget* historyWidget;
    bool                      dragging = false;
    QPoint                    dragStartPosition;
    int                       defaultWidth;
    const int                 expandedWidth = 400;
};