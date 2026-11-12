CREATE TABLE entries (
  id TEXT NOT NULL PRIMARY KEY,
  type TEXT NOT NULL CHECK (type IN ('evening', 'checkin')),
  day_key TEXT NOT NULL,
  energy INTEGER NOT NULL CHECK (energy BETWEEN 1 AND 5),
  positivity INTEGER NOT NULL CHECK (positivity BETWEEN 1 AND 5),
  done TEXT NULL,
  achievement TEXT NULL,
  feelings TEXT NULL,
  checkin_text TEXT NULL,
  created_at_utc TEXT NOT NULL,
  updated_at_utc TEXT NOT NULL,
  timezone_offset INTEGER NOT NULL,
  is_backfilled INTEGER NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX ux_entries_evening_day ON entries(day_key) WHERE type = 'evening';
CREATE INDEX ix_entries_day_key ON entries(day_key);

CREATE TABLE tags (
  id TEXT NOT NULL PRIMARY KEY,
  card_type TEXT NOT NULL CHECK (card_type IN ('done', 'achievement', 'feelings')),
  label TEXT NOT NULL,
  icon TEXT NOT NULL,
  direction TEXT NOT NULL,
  color TEXT NULL,
  sort_order INTEGER NOT NULL,
  use_count INTEGER NOT NULL DEFAULT 0,
  is_custom INTEGER NOT NULL DEFAULT 0,
  is_archived INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE entry_tags (
  entry_id TEXT NOT NULL REFERENCES entries(id) ON DELETE CASCADE,
  tag_id TEXT NOT NULL REFERENCES tags(id),
  card_type TEXT NOT NULL,
  PRIMARY KEY (entry_id, tag_id, card_type)
);

CREATE INDEX ix_entry_tags_tag ON entry_tags(tag_id);

CREATE TABLE reports (
  id TEXT NOT NULL PRIMARY KEY,
  week_start TEXT NOT NULL UNIQUE,
  generated_at_utc TEXT NOT NULL,
  status TEXT NOT NULL,
  days_with_entries INTEGER NOT NULL,
  content TEXT NOT NULL,
  shown_at_utc TEXT NULL
);

CREATE TABLE report_advice (
  id TEXT NOT NULL PRIMARY KEY,
  report_id TEXT NOT NULL REFERENCES reports(id) ON DELETE CASCADE,
  rule_id TEXT NOT NULL,
  direction TEXT NOT NULL,
  text TEXT NOT NULL,
  feedback TEXT NULL,
  sort_order INTEGER NOT NULL
);

CREATE TABLE drafts (
  day_key TEXT NOT NULL PRIMARY KEY,
  content TEXT NOT NULL,
  updated_at_utc TEXT NOT NULL
);

CREATE TABLE letters (
  id TEXT NOT NULL PRIMARY KEY,
  text TEXT NOT NULL,
  created_at_utc TEXT NOT NULL,
  open_date TEXT NOT NULL,
  opened_at_utc TEXT NULL,
  notified INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE settings (
  id INTEGER NOT NULL PRIMARY KEY CHECK (id = 1),
  reminder_enabled INTEGER NOT NULL,
  reminder_time TEXT NOT NULL,
  adaptive_reminder INTEGER NOT NULL,
  theme TEXT NOT NULL,
  user_name TEXT NULL,
  onboarding_done INTEGER NOT NULL,
  dismissed_tag_suggestions TEXT NOT NULL DEFAULT '[]'
);
