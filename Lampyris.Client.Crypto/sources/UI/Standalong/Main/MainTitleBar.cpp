// Project Include(s)
#include "MainTitleBar.h"
#include "UI/Common/SymbolSearchResultWidget.h"
#include "UI/Common/BetterLineEdit.h"

// QT Include(s)
#include <QPropertyAnimation>
#include <QTimer>

MainTitleBar::MainTitleBar(QWidget* parent) : QWidget(parent) {
    setFixedHeight(32); // 设置标题栏高度
    setStyleSheet("MainTitleBar { background-color: #181A20; color: white; }");
    setAttribute(Qt::WA_StyledBackground); // 启用样式表背景支持

    // 主布局
    QHBoxLayout* mainLayout = new QHBoxLayout(this);
    mainLayout->setContentsMargins(5, 0, 5, 0);
    mainLayout->setSpacing(10);

    // 左侧图标
    QLabel* iconLabel = new QLabel(this);
    iconLabel->setPixmap(QPixmap(":/res/icons/lampyris_logo.png").scaled(24, 24, Qt::KeepAspectRatio, Qt::SmoothTransformation));
    iconLabel->setStyleSheet("background-color: transparent;"); // 设置透明背景
    mainLayout->addWidget(iconLabel);

    // 标题文本
    QLabel* titleLabel = new QLabel("Lampyris Crypto Client", this);
    titleLabel->setStyleSheet("font-size: 13px; color: white; background-color: transparent;"); // 设置透明背景
    mainLayout->addWidget(titleLabel);

    // 中间空白区域
    QSpacerItem* spacer = new QSpacerItem(0, 0, QSizePolicy::Expanding, QSizePolicy::Minimum);
    mainLayout->addSpacerItem(spacer);

    // 搜索框
    QVBoxLayout* searchLayout = new QVBoxLayout();
    // 搜索框
    searchBox = new BetterLineEdit(this);
    searchBox->setFocusPolicy(Qt::FocusPolicy::ClickFocus);
    searchBox->setPlaceholderText("Search...");
    searchBox->setFixedSize(200, 25);
    connect(searchBox, &QLineEdit::editingFinished, this, [this]() {
        searchBox->clearFocus(); // 清除焦点
        this->setFocus();
    });

    // 添加搜索图标到 QLineEdit 内部
    QAction* searchIconAction = new QAction(this);
    searchIconAction->setIcon(QIcon(":/res/icons/search.png")); // 设置搜索图标
    searchBox->addAction(searchIconAction, QLineEdit::LeadingPosition); // 添加图标到左侧

    // 设置样式
    searchBox->setStyleSheet(
        "QLineEdit {"
        "    background-color: white;"
        "    color: black;"
        "    border-radius: 5px;"
        "    height: 25px;"
        "}"
        "QLineEdit:focus {"
        "    border: 2px solid orange;"
        "}"
    );
    searchLayout->addWidget(searchBox);

    // 历史记录窗口
    historyWidget = new SymbolSearchResultWidget(this);

    // 搜索框焦点事件处理
    connect(searchBox, &BetterLineEdit::signalInFocus, this, &MainTitleBar::onSearchBoxFocusIn);
    connect(searchBox, &BetterLineEdit::signalOutFocus, this, &MainTitleBar::onSearchBoxFocusOut);
    searchBox->setHistoryWidget(historyWidget);

    // 联想词列表
    suggestionList = new QListWidget(this);
    suggestionList->setStyleSheet("background-color: white; color: black; border: 1px solid gray;");
    suggestionList->setVisible(false); // 默认隐藏
    searchLayout->addWidget(suggestionList);

    mainLayout->addLayout(searchLayout);

    // 中间空白区域
    spacer = new QSpacerItem(0, 0, QSizePolicy::Expanding, QSizePolicy::Minimum);
    mainLayout->addSpacerItem(spacer);

    // 小图标按钮
    QPushButton* iconButton1 = new QPushButton(this);
    iconButton1->setIcon(QIcon(":/res/icons/icon1.png"));
    iconButton1->setFixedSize(32, 32);
    iconButton1->setStyleSheet("border: none; background-color: transparent;");

    QPushButton* iconButton2 = new QPushButton(this);
    iconButton2->setIcon(QIcon(":/res/icons/icon2.png"));
    iconButton2->setFixedSize(32, 32);
    iconButton2->setStyleSheet("border: none; background-color: transparent;");

    QPushButton* iconButton3 = new QPushButton(this);
    iconButton3->setIcon(QIcon(":/res/icons/icon3.png"));
    iconButton3->setFixedSize(32, 32);
    iconButton3->setStyleSheet("border: none; background-color: transparent;");

    // 最小化、最大化和关闭按钮
    QPushButton* minimizeButton = new QPushButton(this);
    minimizeButton->setIcon(QIcon(":/res/icons/minimize.png"));
    minimizeButton->setFixedSize(32, 32);
    minimizeButton->setStyleSheet("border: none; background-color: transparent;");
    connect(minimizeButton, &QPushButton::clicked, this, &MainTitleBar::minimizeWindow);
    mainLayout->addWidget(minimizeButton);

    QPushButton* maximizeButton = new QPushButton(this);
    maximizeButton->setIcon(QIcon(":/res/icons/maximize.png"));
    maximizeButton->setFixedSize(32, 32);
    maximizeButton->setStyleSheet("border: none; background-color: transparent;");
    connect(maximizeButton, &QPushButton::clicked, this, &MainTitleBar::maximizeWindow);
    mainLayout->addWidget(maximizeButton);

    QPushButton* closeButton = new QPushButton(this);
    closeButton->setIcon(QIcon(":/res/icons/close.png"));
    closeButton->setFixedSize(32, 32);
    closeButton->setStyleSheet("border: none; background-color: transparent;");
    connect(closeButton, &QPushButton::clicked, this, &MainTitleBar::closeWindow);
    mainLayout->addWidget(closeButton);

    setLayout(mainLayout);
}

void MainTitleBar::setSuggestions(const QStringList& suggestions) {
    this->suggestions = suggestions;
}

void MainTitleBar::mousePressEvent(QMouseEvent* event) {
    if (parentWidget() != nullptr && event->button() == Qt::LeftButton) {
        dragging = true;
        dragStartPosition = event->globalPosition().toPoint() - parentWidget()->frameGeometry().topLeft();
        event->accept();
    }
}

void MainTitleBar::mouseMoveEvent(QMouseEvent* event) {
    if (parentWidget() != nullptr && dragging && event->buttons() & Qt::LeftButton) {
        parentWidget()->move(event->globalPosition().toPoint() - dragStartPosition);
        event->accept();
    }
}

void MainTitleBar::mouseReleaseEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        dragging = false;
        event->accept();
    }
}

void MainTitleBar::minimizeWindow() {
    parentWidget()->showMinimized();
}

void MainTitleBar::maximizeWindow() {
    if (parentWidget()->isMaximized()) {
        parentWidget()->showNormal();
    }
    else {
        parentWidget()->showMaximized();
    }
}

void MainTitleBar::closeWindow() {
    parentWidget()->close();
}

void MainTitleBar::updateSuggestions(const QString& text) {
    suggestionList->clear();
    if (text.isEmpty()) {
        suggestionList->setVisible(false);
        return;
    }

    for (const QString& suggestion : suggestions) {
        if (suggestion.contains(text, Qt::CaseInsensitive)) {
            suggestionList->addItem(suggestion);
        }
    }

    suggestionList->setVisible(suggestionList->count() > 0);
}

void MainTitleBar::selectSuggestion(QListWidgetItem* item) {
    searchBox->setText(item->text());
    suggestionList->setVisible(false);
}

void MainTitleBar::onSearchBoxFocusIn() {
    // 播放宽度拉大动画
    defaultWidth = searchBox->geometry().width();
    QPropertyAnimation* animation = new QPropertyAnimation(searchBox, "geometry");
    animation->setDuration(300);
    animation->setStartValue(searchBox->geometry());
    animation->setEndValue(QRect(searchBox->x(), searchBox->y(), expandedWidth, searchBox->height()));
    animation->start(QAbstractAnimation::DeleteWhenStopped);

    // 显示历史记录窗口
    historyWidget->setHistory({ "Record 1", "Record 2", "Record 3", "Record 4", "Record 5",
        "Record 6", "Record 7", "Record 8", "Record 9", "Record 10" },
        expandedWidth);
    historyWidget->setGeometry(searchBox->x(), searchBox->y() + searchBox->height() + 5, expandedWidth, 150);
    historyWidget->show();
}

void MainTitleBar::onSearchBoxFocusOut() {
    // 播放宽度缩小动画
    QPropertyAnimation* animation = new QPropertyAnimation(searchBox, "geometry");
    animation->setDuration(300);
    animation->setStartValue(searchBox->geometry());
    animation->setEndValue(QRect(searchBox->x(), searchBox->y(), defaultWidth, searchBox->height()));
    animation->start(QAbstractAnimation::DeleteWhenStopped);

    // 关闭历史记录窗口
    historyWidget->hide();
}
