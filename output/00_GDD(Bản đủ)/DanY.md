DÀN Ý TIỂU LUẬN CHUYÊN NGÀNH

  Đề xuất tựa đề

  "Phát triển Game Auto Chess với Trí tuệ Nhân tạo Dựa trên Thuật toán Di truyền"

  (Subtile có thể thêm: Ứng dụng Genetic Algorithm vào huấn luyện Bot chiến lược)

  ---
  TỔNG QUAN CẤU TRÚC (~80–100 trang)

  ┌──────────┬─────────────────────────────────────────┬───────────────┐
  │  Chương  │                 Tiêu đề                 │ Trang dự kiến │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ Mở đầu   │ Giới thiệu đề tài                       │ 4–6           │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ 1        │ Tổng quan lĩnh vực                      │ 10–14         │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ 2        │ Cơ sở lý thuyết                         │ 16–20         │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ 3        │ Thiết kế game (GDD)                     │ 14–18         │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ 4        │ Kiến trúc hệ thống kỹ thuật             │ 10–14         │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ 5        │ Thuật toán di truyền & hệ thống AI      │ 14–18         │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ 6        │ Kết quả & đánh giá                      │ 8–10          │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ Kết luận │ Tổng kết & hướng phát triển             │ 4–5           │
  ├──────────┼─────────────────────────────────────────┼───────────────┤
  │ Phụ lục  │ References, code snippets, bảng dữ liệu │ 6–10          │
  └──────────┴─────────────────────────────────────────┴───────────────┘

  ---
  CHI TIẾT TỪNG CHƯƠNG

  ---
  MỞ ĐẦU (4–6 trang)

  1. Lý do chọn đề tài
  - Sự phổ biến của thể loại Auto Chess (Dota Auto Chess 2019, TFT, Hearthstone Battlegrounds)
  - Tiềm năng nghiên cứu AI trong môi trường game chiến lược ngẫu nhiên
  - Khoảng trống nghiên cứu: hầu hết AI trong game thương mại dùng rule-based, GA còn ít được ứng dụng

  2. Mục tiêu đề tài
  - Xây dựng game Auto Chess hoàn chỉnh trên Unity
  - Thiết kế hệ thống AI bot thuần túy bằng Genetic Algorithm
  - Huấn luyện ra nhiều archetype bot với phong cách chơi khác biệt

  3. Phạm vi đề tài
  - Nền tảng: Unity 2022+, C#, không dùng ML-Agent hay Neural Network
  - AI hoàn toàn headless (không cần Unity Editor để training)
  - Không mô phỏng multiplayer thực sự

  4. Phương pháp nghiên cứu
  - Nghiên cứu lý thuyết: GA, game design, combat systems
  - Thiết kế & lập trình iterative (agile-style)
  - Thực nghiệm: chạy training, đo fitness, so sánh bot

  5. Cấu trúc báo cáo

  ---
  CHƯƠNG 1: TỔNG QUAN LĨNH VỰC (10–14 trang)

  1.1 Lịch sử và tổng quan thể loại Auto Chess
  - Nguồn gốc: Dota Auto Chess mod (2019) → Teamfight Tactics, Hearthstone Battlegrounds, Underlords
  - Đặc trưng thể loại: draft → place → auto-combat, economy management, synergy building
  - Phân biệt Auto Chess với Card Game, Tower Defense, RTS

  1.2 AI trong game chiến lược
  - Phương pháp truyền thống: Rule-based AI, Decision Tree, Minimax
  - Xu hướng hiện đại: Reinforcement Learning (AlphaStar, OpenAI Five)
  - Hạn chế RL trong Auto Chess: không gian trạng thái quá lớn, reward thưa

  1.3 Genetic Algorithm trong Game AI
  - Các nghiên cứu tiêu biểu: NEAT (Neuroevolution), game balancing via GA
  - Ưu điểm GA cho Auto Chess: không cần labeled data, khả năng evolve playstyle
  - Các game ứng dụng GA thực tế

  1.4 Các công trình liên quan
  - TFT bot research papers (nếu có)
  - Unity ML-Agents framework (và lý do không dùng trong đề tài này)
  - Game simulation frameworks

  1.5 Định vị đề tài
  - Đóng góp mới: Chromosome 37-gene kiểm soát 100% hành vi bot; TTE ability engine; batch training headless

  ---
  CHƯƠNG 2: CƠ SỞ LÝ THUYẾT (16–20 trang)

  2.1 Thuật toán Di truyền (Genetic Algorithm)

  2.1.1 Nền tảng sinh học tiến hóa
  - Quần thể (Population), cá thể (Individual), gen (Gene), nhiễm sắc thể (Chromosome)
  - Chọn lọc tự nhiên (Natural Selection), lai ghép (Crossover), đột biến (Mutation)

  2.1.2 Cấu trúc GA chuẩn
  - Sơ đồ vòng lặp: Initialize → Evaluate → Select → Crossover → Mutate → Replace
  - Encoding: Binary, Real-valued, permutation
  - Selection methods: Roulette Wheel, Tournament Selection, Elitism

  2.1.3 Các tham số quan trọng
  - Population size vs. diversity trade-off
  - Mutation rate: quá thấp → convergence sớm; quá cao → random walk
  - Fitness function design

  2.1.4 Vấn đề convergence và diversity
  - Premature convergence
  - Island model / Sub-populations (liên kết trực tiếp với thiết kế dự án)

  2.2 Lập trình game với Unity

  2.2.1 Kiến trúc Unity: GameObject, Component, MonoBehaviour
  2.2.2 Pattern Singleton và ScriptableObject
  2.2.3 Headless batch mode (không cần màn hình, dùng cho training)
  2.2.4 JSON serialization trong Unity

  2.3 Thiết kế hệ thống Ability (TTE Pattern)
  - Trigger → Target → Effect: event-driven game logic
  - So sánh với ECS (Entity Component System)
  - Data-driven design: toàn bộ card định nghĩa bằng JSON

  2.4 Lý thuyết game kinh tế trong Auto Chess
  - Interest economy: giữ tiền nhiều → lãi cao
  - Level investment: tăng tier → pool tốt hơn, nhiều slot board hơn
  - Tempo vs. Economy: mua sớm vs. tiết kiệm

  ---
  CHƯƠNG 3: THIẾT KẾ GAME – GAME DESIGN DOCUMENT (14–18 trang)

  3.1 Tầm nhìn và Concept
  - Tựa game, thể loại, nền tảng mục tiêu
  - Core fantasy: "Tôi là chiến lược gia triệu hồi và điều phối các vị thần cổ đại"
  - Target audience

  3.2 Vòng lặp gameplay cốt lõi (Core Loop)
  - Sơ đồ: Shop Phase → Place Phase → Combat Phase → Result → (repeat)
  - Turn structure: 7 sub-turns shop, 20 sub-turns combat
  - Điều kiện thắng/thua: tích lũy 10 cups / HP về 0

  3.3 Hệ thống Bài (Card System)

  3.3.1 Phân loại card
  - Unit cards vs. Spell cards
  - Thuộc tính: ATK, HP, Cost (1–3), Tier (1–3), Tribe

  3.3.2 Hệ thống Bộ tộc (Tribe Synergy)
  - Babylon: buff HP vĩnh viễn khi đủ 3 unit
  - Olympus: buff ATK
  - Niles: buff HP ≥3 unit
  - Hiệu ứng global: áp dụng lên tất cả unit cùng tribe (hand, board, shop, tương lai)

  3.3.3 Passive Keywords
  - Taunt: bắt buộc bị nhắm mục tiêu
  - Reborn: hồi sinh với 1 HP sau khi chết
  - Safeguard: giảm/chặn damage

  3.3.4 Hệ thống Ability (TTE)
  - 14 Trigger Types: StartOfBattle, OnDeath, OnAttack, OnTakeDamage, EndTurnShop, OnDeploy, OnSell, OnAllyDeath,
  OnAllySummon, OnAllyReborn, Aura, OnStatGain, OnAllyDeploy, OnAllySell
  - 12 Target Types: Self, RandomAlly, AllAllies, DirectEnemy, LowestHealthAlly, v.v.
  - 13 Effect Types: AddStats, DealDamage, GiveBuff, Summon, Destroy, GainCoin, Reborn, TriggerAbility, SummonConsumed,
  GiveCard, AbsorbStats, GiveStats, ScaleTargetStats

  3.3.5 Hệ thống Merge
  - 3 bản sao → tier up (ATK × tier, HP × tier)
  - Merge hint system: highlight khi có ≥2 bản sao trên board/hand

  3.4 Hệ thống Shop
  - Pool 5 card ngẫu nhiên mỗi lượt
  - Reroll: 1 coin
  - Freeze: giữ shop sang lượt sau
  - Tier progression: tự động theo turn (max tier 6)
  - Drop rate: weighted theo tier hiện tại

  3.5 Hệ thống Kinh tế
  - Coin income: 3 base + bonus (liên thắng/liên thua) + interest (max 5 coin/lượt nếu giữ ≥50 coin)
  - Spell economy: mua spell để buff unit thay vì mua unit mới

  3.6 Hệ thống Chiến đấu (Combat)
  - Board 7 slot (2 hàng: frontline / backline)
  - Turn-based combat: unit đánh theo thứ tự dynamic sorted queue (ATK speed)
  - Ability resolution: death stack engine – xử lý cái chết theo thứ tự stack
  - 20 combat turns → nếu hòa, không ai mất HP

  3.7 Cân bằng game (Balancing)
  - Phân tích spread cost/tier của 68 card
  - Tại sao Reborn + Taunt combo nguy hiểm
  - Điều chỉnh qua quá trình test

  3.8 UI/UX Design
  - Màn hình chọn độ khó
  - HUD gameplay: HP, coins, turn counter, cups
  - Card slot drag & drop
  - Sell zone
  - Background thay đổi theo phase (shop / combat)
  - Card detail panel

  ---
  CHƯƠNG 4: KIẾN TRÚC HỆ THỐNG KỸ THUẬT (10–14 trang)

  4.1 Tổng quan kiến trúc
  - Sơ đồ module (diagram)
  - Phân tách: Core Engine / Game Logic / AI System / UI Layer
  - Nguyên tắc thiết kế: data-driven, headless-compatible, no circular dependency

  4.2 Data Layer – Hệ thống dữ liệu

  4.2.1 CardDefinition & AbilityData
  - JSON-driven: CardsData.json (68 cards, 1328 dòng)
  - Cấu trúc CardDefinition: id, name, baseATK, baseHP, cost, tier, tribe, keywords, abilities[]
  - Cấu trúc AbilityData: triggerType, targetType, effectType, value, condition, limit

  4.2.2 CardInstance – Runtime state
  - Phân biệt Definition (template) vs. Instance (runtime)
  - currentATK/HP, mergeLevel, passiveList, consumedCards[]

  4.3 Ability Engine – TTE Pattern

  4.3.1 Kiến trúc 3 lớp: Trigger → Target → Effect
  4.3.2 AbilityEngine.cs: Event dispatch, ability resolution chain
  4.3.3 AbilityEngine.Targets.cs: Target selection algorithms
  4.3.4 Xử lý edge case: Reborn chain, SummonConsumed, TriggerAbility (đệ quy)

  4.4 Combat Engine

  4.4.1 CombatResolver.cs: vòng lặp chiến đấu
  4.4.2 Death Stack: xử lý cái chết không ngay lập tức, gom vào stack, resolve cuối turn
  4.4.3 Dynamic sorted queue thay thế Stack cho attack order (commit 092b524)
  4.4.4 Pre-combat snapshot: lưu board state trước combat, khôi phục sau

  4.5 Manager Layer

  4.5.1 GameManager: Singleton, state machine (Shop/Combat/Result)
  4.5.2 Partial class pattern: GameManager.Shop.cs, .Combat.cs, .Board.cs
  4.5.3 CardDatabase: weighted random pool, drop rate tables
  4.5.4 EconomyManager: interest formula, coin flow

  4.6 UI Architecture

  4.6.1 CardUI + CardDraggable + CardSlot: drag-and-drop chain  
  4.6.2 UIManager: panel management, event binding
  4.6.3 GameRecord + FlashType: replay & visual feedback system

  4.7 Headless Compatibility
  - Tách AI hoàn toàn khỏi MonoBehaviour
  - GameSimulator không cần scene, không cần Camera
  - Cho phép chạy batch training qua train_ai.ps1

  ---
  CHƯƠNG 5: THUẬT TOÁN DI TRUYỀN & HỆ THỐNG AI (14–18 trang)

  5.1 Tổng quan hệ thống AI
  - Mục tiêu: tạo ra bot AI chơi giống người, có playstyle khác nhau
  - Không dùng rule-based hay Neural Network – 100% GA
  - Kết quả: 5 specialist bots (hardBot, babylonBot, nileBot, summonerBot, resilientBot)

  5.2 Thiết kế Chromosome – 37 Gene

  (Đây là phần trọng tâm, dùng bảng + hình minh họa)

  5.2.1 Nhóm đánh giá card cơ bản [Gene 0–3]

  ┌──────┬────────────┬────────────────────────────────┐
  │ Gene │    Tên     │            Ý nghĩa             │
  ├──────┼────────────┼────────────────────────────────┤
  │ 0    │ wATK       │ Trọng số ATK khi đánh giá card │
  ├──────┼────────────┼────────────────────────────────┤
  │ 1    │ wHP        │ Trọng số HP                    │
  ├──────┼────────────┼────────────────────────────────┤
  │ 2    │ wTierBonus │ Bonus mỗi tier vượt 1          │
  ├──────┼────────────┼────────────────────────────────┤
  │ 3    │ wCostEff   │ Hiệu quả chi phí (stat/coin)   │
  └──────┴────────────┴────────────────────────────────┘

  5.2.2 Nhóm Passive Keywords [Gene 4–6]
  5.2.3 Nhóm Trigger Weights [Gene 7–12]
  5.2.4 Nhóm Effect Weights [Gene 13–17]
  5.2.5 Nhóm Tribe Synergy [Gene 18–20]
  5.2.6 Nhóm Board Context [Gene 21–23]
  5.2.7 Nhóm Reroll Behavior [Gene 24–27]
  5.2.8 Nhóm Spell Behavior [Gene 28–31]
  5.2.9 Nhóm Trigger Con Độc lập [Gene 32–36]

  5.3 BotAgent – Bộ não quyết định

  5.3.1 7 Phase Decision Pipeline
  Phase 1: Reroll evaluation (gene 24–27)
  Phase 2: Buy units (score = wATK×ATK + wHP×HP + ...)
  Phase 3: Buy & cast spells (gene 28–31)
  Phase 4: Proactive sell (gene 27)
  Phase 5: Merge evaluation (gene 21)
  Phase 6: Reposition (gene 22 – Taunt frontline)
  Phase 7: Freeze shop decision

  5.3.2 Card Scoring Function
  - Công thức tính điểm: S = wATK·ATK + wHP·HP + wTierBonus·(tier−1) + passive bonuses + trigger weights + effect
  weights + tribe bonus

  5.3.3 Economy Decision Logic
  - When to reroll vs. save vs. spend

  5.4 GameSimulator – Môi trường huấn luyện

  5.4.1 Match simulation: 2 bot chơi 20 turn
  5.4.2 Fitness function
  - Win rate × 0.6 + HP dealt ratio × 0.3 + survival turns × 0.1
  - Normalization cross-population

  5.5 GATrainer – Vòng lặp tiến hóa

  5.5.1 Khởi tạo quần thể
  - Population 30–120 cá thể
  - 5 sub-populations (Island Model): Babylon, Niles, Summoner, Resilient, Random
  - Seed chromosomes cho mỗi archetype

  5.5.2 Evaluation
  - Tournament: mỗi chromosome thi đấu với N đối thủ ngẫu nhiên trong population
  - Parallel simulation (không có side effects, thread-safe)

  5.5.3 Selection: Tournament Selection (k=3)
  5.5.4 Crossover: Single-point crossover, uniform crossover
  5.5.5 Mutation: Gaussian noise (rate 8–12%)
  5.5.6 Immigration: inject random individuals mỗi 10 generation → chống premature convergence

  5.6 AILibrary – Kết quả training

  5.6.1 Lưu trữ: AI_Library.json với 5 specialist chromosomes
  5.6.2 Difficulty mapping: Easy/Medium/Hard → chọn bot
  5.6.3 Phân tích gene profile của từng bot (radar chart)

  5.7 Kết quả Thực nghiệm Training

  5.7.1 Thiết lập thí nghiệm
  - Quick mode: 30 pop × 40 gen (~2 phút)
  - Production mode: 100+ pop × 150 gen (~20 phút)

  5.7.2 Fitness convergence curve (biểu đồ)
  5.7.3 So sánh playstyle giữa 5 bots:
  - babylonBot: ưu tiên tribe Babylon, HP-heavy
  - nileBot: ưu tiên tribe Niles, tốc độ grow HP
  - summonerBot: summon chain + reborn/consume
  - resilientBot: Taunt + Reborn defensive
  - hardBot: balanced generalist

  5.7.4 Win rate matrix giữa các bots (5×5 table)

  ---
  CHƯƠNG 6: KẾT QUẢ VÀ ĐÁNH GIÁ (8–10 trang)

  6.1 Demo game – Screenshots
  - Màn hình chọn độ khó
  - Shop phase (mid-game)
  - Combat phase (animation/board state)
  - Win/lose screen

  6.2 Đánh giá hệ thống game

  6.2.1 Tính hoàn chỉnh của mechanic
  - Checklist: 14 trigger types tested, 13 effects tested, merge working, tribe synergy working
  - Edge cases: Reborn chain, SummonConsumed loop, Aura passive

  6.2.2 Hiệu năng
  - Combat simulation: time/match
  - Training: time/generation

  6.3 Đánh giá hệ thống AI

  6.3.1 So sánh bot vs. random agent
  6.3.2 Độ khó: Easy bot win rate vs. người chơi mới
  6.3.3 Hard bot behavior analysis: chromosome gene values

  6.4 Hạn chế và điểm cần cải thiện
  - Thiếu multiplayer thực sự (hiện tại PvE only)
  - Chromosome real-valued có thể bị stuck in local optima
  - UI chưa có animation chiến đấu trực quan (combat là headless)

  6.5 Bài học kinh nghiệm
  - Tại sao chọn GA thay vì MCTS hay RL
  - Khó khăn khi thiết kế fitness function
  - Tầm quan trọng của Island Model trong GA

  ---
  KẾT LUẬN (4–5 trang)

  1. Tổng kết đóng góp
  - Game Auto Chess hoàn chỉnh: 68 card, 3 bộ tộc, TTE ability engine, economy system
  - Hệ thống AI thuần GA: 37-gene chromosome, 5 archetypes, headless training
  - Kiến trúc data-driven: toàn bộ card và AI lưu JSON, dễ mở rộng

  2. Hướng phát triển tương lai
  - Thêm multiplayer (PvP local/online)
  - Nâng chromosome lên Neural-GA (NEAT)
  - Mobile port
  - Thêm 2–3 bộ tộc mới, mở rộng pool lên 100+ card
  - Replay system đầy đủ

  3. Kết luận cá nhân
  - Trải nghiệm học thuật và kỹ thuật từ dự án
  - Ứng dụng thực tế của GA trong game development

  ---
  PHỤ LỤC

  - Phụ lục A: Danh sách đầy đủ 68 card (bảng: ID, tên, cost, tier, tribe, abilities)
  - Phụ lục B: Chromosome template – 37 gene definition table
  - Phụ lục C: Giá trị gene của 5 trained bots (bảng so sánh)
  - Phụ lục D: Code snippet quan trọng (BotAgent scoring, GA crossover, TTE dispatch)
  - Phụ lục E: Tài liệu tham khảo (IEEE papers về GA game AI, TFT research, Unity docs)

  ---
  GỢI Ý VỀ HÌNH ẢNH (để đạt 80+ trang)

  ┌─────────┬───────────────────────────────────────────────────────────────────────────────────────────────────────┐
  │ Vị trí  │                                           Hình ảnh đề xuất                                            │
  ├─────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Chương  │ Screenshot TFT, Hearthstone Battlegrounds để so sánh                                                  │
  │ 1       │                                                                                                       │
  ├─────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Chương  │ Sơ đồ GA standard flow, diagram TTE pattern                                                           │
  │ 2       │                                                                                                       │
  ├─────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Chương  │ Board layout diagram, card anatomy, tribe synergy chart, core loop flowchart, UI                      │
  │ 3       │ wireframes/screenshots                                                                                │
  ├─────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Chương  │ Architecture diagram (module layers), class diagram (CardDefinition/Instance), death stack diagram,   │
  │ 4       │ combat flow chart                                                                                     │
  ├─────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Chương  │ Chromosome structure table (màu sắc từng nhóm gene), BotAgent 7-phase flow, fitness convergence       │
  │ 5       │ graph, win rate matrix, radar chart so sánh 5 bots                                                    │
  ├─────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Chương  │ 4–6 screenshot gameplay, combat board state, training log output                                      │
  │ 6       │                                                                                                       │
  └─────────┴───────────────────────────────────────────────────────────────────────────────────────────────────────┘

  ---
  PHÂN CÔNG VIẾT (ước tính)

  ┌─────────────────────────┬────────────┬────────────────────────┐
  │          Phần           │ Độ ưu tiên │        Ghi chú         │
  ├─────────────────────────┼────────────┼────────────────────────┤
  │ Chương 2 (Lý thuyết GA) │ Cao nhất   │ Nền tảng học thuật     │
  ├─────────────────────────┼────────────┼────────────────────────┤
  │ Chương 3 (GDD)          │ Cao        │ Nhiều ảnh, cần viết kỹ │
  ├─────────────────────────┼────────────┼────────────────────────┤
  │ Chương 5 (GA & AI)      │ Cao nhất   │ Core contribution      │
  ├─────────────────────────┼────────────┼────────────────────────┤
  │ Chương 4 (Kiến trúc)    │ Cao        │ Nhiều sơ đồ            │
  ├─────────────────────────┼────────────┼────────────────────────┤
  │ Chương 1 & 6            │ Trung bình │ Research + results     │
  ├─────────────────────────┼────────────┼────────────────────────┤
  │ Mở đầu & Kết luận       │ Thấp       │ Viết sau cùng          │
  └─────────────────────────┴────────────┴────────────────────────┘

  ---
  Dàn ý này đủ để tạo ra khoảng 85–105 trang khi viết đầy đủ với hình ảnh. Bạn muốn tôi bắt đầu viết chi tiết chương nào
  trước, hay cần điều chỉnh cấu trúc nào không?