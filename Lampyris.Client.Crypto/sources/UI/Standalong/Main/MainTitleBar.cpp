// Project Include(s)
#include "MainTitleBar.h"
#include "UI/Common/SymbolSearchResultWidget.h"
#include "UI/Common/BetterLineEdit.h"

// QT Include(s)
#include <QPropertyAnimation>
#include <QTimer>

MainTitleBar::MainTitleBar(QWidget* parent) : QWidget(parent) {
    setFixedHeight(32); // ���ñ������߶�
    setStyleSheet("MainTitleBar { background-color: #181A20; color: white; }");
    setAttribute(Qt::WA_StyledBackground); // ������ʽ����֧��

    // ������
    QHBoxLayout* mainLayout = new QHBoxLayout(this);
    mainLayout->setContentsMargins(5, 0, 5, 0);
    mainLayout->setSpacing(10);

    // ���ͼ��
    QLabel* iconLabel = new QLabel(this);
    iconLabel->setPixmap(QPixmap(":/res/icons/lampyris_logo.png").scaled(24, 24, Qt::KeepAspectRatio, Qt::SmoothTransformation));
    iconLabel->setStyleSheet("background-color: transparent;"); // ����͸������
    mainLayout->addWidget(iconLabel);

    // �����ı�
    QLabel* titleLabel = new QLabel("Lampyris Crypto Client", this);
    titleLabel->setStyleSheet("font-size: 13px; color: white; background-color: transparent;"); // ����͸������
    mainLayout->addWidget(titleLabel);

    // �м�հ�����
    QSpacerItem* spacer = new QSpacerItem(0, 0, QSizePolicy::Expanding, QSizePolicy::Minimum);
    mainLayout->addSpacerItem(spacer);

    // ������
    QVBoxLayout* searchLayout = new QVBoxLayout();
    // ������
    searchBox = new BetterLineEdit(this);
    searchBox->setFocusPolicy(Qt::FocusPolicy::ClickFocus);
    searchBox->setPlaceholderText("Search...");
    searchBox->setFixedSize(200, 25);
    connect(searchBox, &QLineEdit::editingFinished, this, [this]() {
        searchBox->clearFocus(); // �������
        this->setFocus();
    });

    // �������ͼ�굽 QLineEdit �ڲ�
    QAction* searchIconAction = new QAction(this);
    searchIconAction->setIcon(QIcon(":/res/icons/search.png")); // ��������ͼ��
    searchBox->addAction(searchIconAction, QLineEdit::LeadingPosition); // ���ͼ�굽���

    // ������ʽ
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

    // ��ʷ��¼����
    historyWidget = new SymbolSearchResultWidget(this);

    // �����򽹵��¼�����
    connect(searchBox, &BetterLineEdit::signalInFocus, this, &MainTitleBar::onSearchBoxFocusIn);
    connect(searchBox, &BetterLineEdit::signalOutFocus, this, &MainTitleBar::onSearchBoxFocusOut);
    searchBox->setHistoryWidget(historyWidget);

    // ������б�
    suggestionList = new QListWidget(this);
    suggestionList->setStyleSheet("background-color: white; color: black; border: 1px solid gray;");
    suggestionList->setVisible(false); // Ĭ������
    searchLayout->addWidget(suggestionList);

    mainLayout->addLayout(searchLayout);

    // �м�հ�����
    spacer = new QSpacerItem(0, 0, QSizePolicy::Expanding, QSizePolicy::Minimum);
    mainLayout->addSpacerItem(spacer);

    // Сͼ�갴ť
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

    // ��С������󻯺͹رհ�ť
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
    // ���ſ�����󶯻�
    defaultWidth = searchBox->geometry().width();
    QPropertyAnimation* animation = new QPropertyAnimation(searchBox, "geometry");
    animation->setDuration(300);
    animation->setStartValue(searchBox->geometry());
    animation->setEndValue(QRect(searchBox->x(), searchBox->y(), expandedWidth, searchBox->height()));
    animation->start(QAbstractAnimation::DeleteWhenStopped);

    // ��ʾ��ʷ��¼����
    historyWidget->setHistory({ "Record 1", "Record 2", "Record 3", "Record 4", "Record 5",
        "Record 6", "Record 7", "Record 8", "Record 9", "Record 10" },
        expandedWidth);
    historyWidget->setGeometry(searchBox->x(), searchBox->y() + searchBox->height() + 5, expandedWidth, 150);
    historyWidget->show();
}

void MainTitleBar::onSearchBoxFocusOut() {
    // ���ſ����С����
    QPropertyAnimation* animation = new QPropertyAnimation(searchBox, "geometry");
    animation->setDuration(300);
    animation->setStartValue(searchBox->geometry());
    animation->setEndValue(QRect(searchBox->x(), searchBox->y(), defaultWidth, searchBox->height()));
    animation->start(QAbstractAnimation::DeleteWhenStopped);

    // �ر���ʷ��¼����
    historyWidget->hide();
}
